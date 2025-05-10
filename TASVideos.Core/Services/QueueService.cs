using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.MovieParsers.Result;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Services;

public interface IQueueService
{
	/// <summary>
	/// Returns a list of all available statuses a submission could be set to
	/// Based on the user's permissions, submission status and date, and authors.
	/// </summary>
	ICollection<SubmissionStatus> AvailableStatuses(
		SubmissionStatus currentStatus,
		IEnumerable<PermissionTo> userPermissions,
		DateTime submitDate,
		bool isAuthorOrSubmitter,
		bool isJudge,
		bool isPublisher);

	int HoursRemainingForJudging(ISubmissionDisplay submission);

	/// <summary>
	/// Returns whether a submission can be deleted, does not affect the submission
	/// </summary>
	Task<DeleteSubmissionResult> CanDeleteSubmission(int submissionId);

	/// <summary>
	/// Deletes a submission permanently
	/// </summary>
	Task<DeleteSubmissionResult> DeleteSubmission(int submissionId);

	/// <summary>
	/// Returns whether a publication can be unpublished, does not affect the publication
	/// </summary>
	Task<UnpublishResult> CanUnpublish(int publicationId);

	/// <summary>
	/// Deletes a publication and returns the corresponding submission back to the submission queue
	/// </summary>
	Task<UnpublishResult> Unpublish(int publicationId);

	/// <summary>
	/// Writes the parsed values from the <see cref="IParseResult"/> into the given submission
	/// </summary>
	/// <returns>The error message if an error occurred, else an empty string</returns>
	Task<string> MapParsedResult(IParseResult parseResult, Submission submission);

	/// <summary>
	/// Obsoletes a publication with the existing publication.
	/// In addition, it marks and syncs the obsoleted YouTube videos
	/// </summary>
	/// <param name="publicationToObsolete">The movie to obsolete</param>
	/// <param name="obsoletingPublicationId">The movie that obsoletes it</param>
	/// <returns>False if publications is not found</returns>
	Task<bool> ObsoleteWith(int publicationToObsolete, int obsoletingPublicationId);

	/// <summary>
	/// Returns whether the user has exceeded the submission limit
	/// </summary>
	/// <returns>Next time the user can submit, if limit has been exceeded, else null</returns>
	Task<DateTime?> ExceededSubmissionLimit(int userId);

	/// <summary>
	/// Returns the total numbers of submissions the given user has submitted
	/// </summary>
	Task<int> GetSubmissionCount(int userId);
}

internal class QueueService(
	AppSettings settings,
	ApplicationDbContext db,
	IYoutubeSync youtubeSync,
	ITASVideoAgent tva,
	IWikiPages wikiPages)
	: IQueueService
{
	private readonly int _minimumHoursBeforeJudgment = settings.MinimumHoursBeforeJudgment;

	public ICollection<SubmissionStatus> AvailableStatuses(
		SubmissionStatus currentStatus,
		IEnumerable<PermissionTo> userPermissions,
		DateTime submitDate,
		bool isAuthorOrSubmitter,
		bool isJudge,
		bool isPublisher)
	{
		// Published submissions can not be changed
		if (currentStatus == Published)
		{
			return [Published];
		}

		var perms = userPermissions.ToList();
		if (perms.Contains(PermissionTo.OverrideSubmissionConstraints))
		{
			return Enum.GetValues<SubmissionStatus>()
				.Except([Published]) // Published status must only be set when being published
				.ToList();
		}

		var list = new HashSet<SubmissionStatus>
			{
				currentStatus // The current status must always be in the list
			};

		var canJudge = perms.Contains(PermissionTo.JudgeSubmissions);
		var canPublish = perms.Contains(PermissionTo.PublishMovies);
		var isAfterJudgmentWindow = submitDate < DateTime.UtcNow.AddHours(-_minimumHoursBeforeJudgment);

		if (isJudge && currentStatus == JudgingUnderWay // The judge can set back to new if they claimed the submission and are now opting out
			|| currentStatus == Rejected && isJudge // A judge can revive a rejected submission by setting it to new
			|| currentStatus == Accepted && isJudge // A judge can undo their judgment
			|| currentStatus == PublicationUnderway && isJudge // A judge can undo even if publication underway
			|| isJudge && currentStatus == Delayed // Judges can set delayed -> new
			|| isJudge && currentStatus == NeedsMoreInfo // Judges can set info -> new
			|| (isJudge || isAuthorOrSubmitter) && currentStatus == Cancelled
			|| isJudge && currentStatus == Playground)
		{
			list.Add(New);
		}

		// A judge can claim a new run, unless they are not author or the submitter
		if (new[] { New, JudgingUnderWay, Delayed, NeedsMoreInfo, Accepted, Rejected, PublicationUnderway, Cancelled, Playground }.Contains(currentStatus)
			&& canJudge
			&& !isAuthorOrSubmitter)
		{
			list.Add(JudgingUnderWay);
		}

		// A judge can set a submission to delayed or needs more info so long as they have claimed it
		if (new[] { JudgingUnderWay, Delayed, NeedsMoreInfo, Accepted, PublicationUnderway }.Contains(currentStatus)
			&& isJudge
			&& isAfterJudgmentWindow)
		{
			list.Add(JudgingUnderWay);
			list.Add(Delayed);
			list.Add(NeedsMoreInfo);
		}

		// A judge can deliver a verdict if they have claimed the submission
		if (new[] { JudgingUnderWay, Delayed, NeedsMoreInfo, PublicationUnderway }.Contains(currentStatus)
			&& isJudge
			&& isAfterJudgmentWindow)
		{
			list.Add(Accepted);
			list.Add(Rejected);
		}
		else if (currentStatus is Accepted or PublicationUnderway
			&& isJudge
			&& isAfterJudgmentWindow)
		{
			list.Add(Rejected); // A judge can overrule themselves and reject an accepted movie
		}

		// A publisher can set it to publication underway if it has been accepted
		if (currentStatus == Accepted && canPublish)
		{
			list.Add(PublicationUnderway);
		}

		// A publisher needs to be able to retract their publishing claim
		if (currentStatus == PublicationUnderway && isPublisher)
		{
			list.Add(Accepted);
		}

		// An author or a judge can cancel as long as the submission has not been published
		if ((isJudge || isAuthorOrSubmitter)
			&& new[] { New, JudgingUnderWay, Delayed, NeedsMoreInfo, Accepted, PublicationUnderway }.Contains(currentStatus))
		{
			list.Add(Cancelled);
		}

		if (new[] { JudgingUnderWay, Delayed, NeedsMoreInfo }.Contains(currentStatus)
			&& isJudge
			&& isAfterJudgmentWindow)
		{
			list.Add(Playground);
		}

		return list;
	}

	public int HoursRemainingForJudging(ISubmissionDisplay submission)
	{
		if (!submission.Status.CanBeJudged())
		{
			return 0;
		}

		var diff = (DateTime.UtcNow - submission.Date).TotalHours;
		return _minimumHoursBeforeJudgment - (int)diff;
	}

	public async Task<DeleteSubmissionResult> CanDeleteSubmission(int submissionId)
	{
		var sub = await db.Submissions
			.Where(s => s.Id == submissionId)
			.Select(s => new
			{
				s.Title,
				IsPublished = s.PublisherId.HasValue
			})
			.SingleOrDefaultAsync();

		if (sub is null)
		{
			return DeleteSubmissionResult.NotFound();
		}

		if (sub.IsPublished)
		{
			return DeleteSubmissionResult.IsPublished(sub.Title);
		}

		return DeleteSubmissionResult.Success(sub.Title);
	}

	public async Task<DeleteSubmissionResult> DeleteSubmission(int submissionId)
	{
		var submission = await db.Submissions
			.Include(s => s.SubmissionAuthors)
			.Include(s => s.History)
			.SingleOrDefaultAsync(s => s.Id == submissionId);

		if (submission is null)
		{
			return DeleteSubmissionResult.NotFound();
		}

		if (submission.PublisherId.HasValue)
		{
			return DeleteSubmissionResult.IsPublished(submission.Title);
		}

		submission.SubmissionAuthors.Clear();
		submission.History.Clear();
		db.Submissions.Remove(submission);
		if (submission.TopicId.HasValue)
		{
			var topic = await db.ForumTopics
				.Include(t => t.ForumPosts)
				.Include(t => t.Poll)
				.ThenInclude(p => p!.PollOptions)
				.ThenInclude(o => o.Votes)
				.SingleAsync(t => t.Id == submission.TopicId);

			db.ForumPosts.RemoveRange(topic.ForumPosts);
			if (topic.Poll is not null)
			{
				db.ForumPollOptionVotes.RemoveRange(topic.Poll.PollOptions.SelectMany(po => po.Votes));
				db.ForumPollOptions.RemoveRange(topic.Poll.PollOptions);
				db.ForumPolls.Remove(topic.Poll);
			}

			db.ForumTopics.Remove(topic);
		}

		await db.SaveChangesAsync();
		await wikiPages.Delete(WikiHelper.ToSubmissionWikiPageName(submissionId));

		return DeleteSubmissionResult.Success(submission.Title);
	}

	public async Task<UnpublishResult> CanUnpublish(int publicationId)
	{
		var pub = await db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => new
			{
				p.Title,
				HasAwards = p.PublicationAwards.Any()
			})
			.SingleOrDefaultAsync();

		if (pub is null)
		{
			return UnpublishResult.NotFound();
		}

		if (pub.HasAwards)
		{
			return UnpublishResult.HasAwards(pub.Title);
		}

		return UnpublishResult.Success(pub.Title);
	}

	public async Task<UnpublishResult> Unpublish(int publicationId)
	{
		var publication = await db.Publications
			.Include(p => p.PublicationAwards)
			.Include(p => p.Authors)
			.Include(p => p.Files)
			.Include(p => p.PublicationFlags)
			.Include(p => p.PublicationRatings)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationUrls)
			.Include(p => p.Submission)
			.Include(p => p.ObsoletedMovies)
			.ThenInclude(o => o.PublicationUrls)
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (publication is null)
		{
			return UnpublishResult.NotFound();
		}

		if (publication.PublicationAwards.Any())
		{
			return UnpublishResult.HasAwards(publication.Title);
		}

		var youtubeUrls = publication.PublicationUrls
			.ThatAreStreaming()
			.Select(pu => pu.Url)
			.Where(youtubeSync.IsYoutubeUrl)
			.ToList();

		var obsoletedPubsWithYoutube = publication.ObsoletedMovies
			.Where(p => p.PublicationUrls.Any(u => youtubeSync.IsYoutubeUrl(u.Url)))
			.ToList();

		publication.Authors.Clear();
		publication.Files.Clear();
		publication.PublicationFlags.Clear();
		publication.PublicationRatings.Clear();
		publication.PublicationTags.Clear();
		publication.PublicationUrls.Clear();
		var logs = await db.PublicationMaintenanceLogs
			.Where(l => l.PublicationId == publicationId)
			.ToListAsync();
		db.RemoveRange(logs);

		// Note: Cascading deletes will ensure obsoleted publications are no longer obsoleted
		db.Publications.Remove(publication);
		db.SubmissionStatusHistory.Add(publication.SubmissionId, publication.Submission!.Status);
		publication.Submission.Status = PublicationUnderway;

		await tva.PostSubmissionUnpublished(publication.SubmissionId);
		await db.SaveChangesAsync();
		await wikiPages.Delete(WikiHelper.ToPublicationWikiPageName(publicationId));

		foreach (var url in youtubeUrls)
		{
			await youtubeSync.UnlistVideo(url!);
		}

		foreach (var obsoletedPub in obsoletedPubsWithYoutube)
		{
			// Re-query to get all the includes
			// We can afford these extra trips, compared to the massive query it would be
			// for a single trip
			var queriedPub = await db.Publications
				.Include(p => p.PublicationUrls)
				.Include(p => p.System)
				.Include(p => p.Game)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.SingleAsync(p => p.Id == obsoletedPub.Id);

			var obsoletedWiki = await wikiPages.PublicationPage(queriedPub.Id);

			foreach (var url in obsoletedPub.PublicationUrls.Where(u => youtubeSync.IsYoutubeUrl(u.Url)))
			{
				await youtubeSync.SyncYouTubeVideo(new YoutubeVideo(
					queriedPub.Id,
					queriedPub.CreateTimestamp,
					url.Url!,
					url.DisplayName,
					queriedPub.Title,
					obsoletedWiki!,
					queriedPub.System!.Code,
					queriedPub.Authors.OrderBy(pa => pa.Ordinal).Select(a => a.Author!.UserName),
					queriedPub.ObsoletedById));
			}
		}

		return UnpublishResult.Success(publication.Title);
	}

	public async Task<string> MapParsedResult(IParseResult parseResult, Submission submission)
	{
		if (!parseResult.Success)
		{
			throw new InvalidOperationException("Cannot mapped failed parse result.");
		}

		var system = await db.GameSystems
			.ForCode(parseResult.SystemCode)
			.SingleOrDefaultAsync();

		if (system is null)
		{
			return $"Unknown system type of {parseResult.SystemCode}";
		}

		submission.MovieStartType = (int)parseResult.StartType;
		submission.Frames = parseResult.Frames;
		submission.RerecordCount = parseResult.RerecordCount;
		submission.MovieExtension = parseResult.FileExtension;
		submission.System = system;
		submission.CycleCount = parseResult.CycleCount;
		submission.Annotations = parseResult.Annotations.CapAndEllipse(3500);
		var warnings = parseResult.Warnings.ToList();
		submission.Warnings = null;
		if (warnings.Any())
		{
			submission.Warnings = string.Join(",", warnings).Cap(500);
		}

		if (parseResult.FrameRateOverride.HasValue)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			var frameRate = await db.GameSystemFrameRates
				.ForSystem(submission.System.Id)
				.FirstOrDefaultAsync(sf => sf.FrameRate == parseResult.FrameRateOverride.Value);

			if (frameRate is null)
			{
				frameRate = new GameSystemFrameRate
				{
					System = submission.System,
					FrameRate = parseResult.FrameRateOverride.Value,
					RegionCode = parseResult.Region.ToString().ToUpper()
				};
				db.GameSystemFrameRates.Add(frameRate);
				await db.SaveChangesAsync();
			}

			submission.SystemFrameRate = frameRate;
		}
		else
		{
			// SingleOrDefault should work here because the only time there could be more than one for a system and region are formats that return a framerate override
			// Those systems should never hit this code block.  But just in case.
			submission.SystemFrameRate = await db.GameSystemFrameRates
				.ForSystem(submission.System.Id)
				.ForRegion(parseResult.Region.ToString().ToUpper())
				.FirstOrDefaultAsync();
		}

		return "";
	}

	public async Task<bool> ObsoleteWith(int publicationToObsolete, int obsoletingPublicationId)
	{
		var toObsolete = await db.Publications
			.Include(p => p.PublicationUrls)
			.Include(p => p.System)
			.Include(p => p.Game)
			.Include(p => p.Authors)
			.ThenInclude(pa => pa.Author)
			.SingleOrDefaultAsync(p => p.Id == publicationToObsolete);

		if (toObsolete is null)
		{
			return false;
		}

		var pageName = WikiHelper.ToPublicationWikiPageName(toObsolete.Id);
		var wikiPage = await wikiPages.Page(pageName);

		toObsolete.ObsoletedById = obsoletingPublicationId;
		await db.SaveChangesAsync();

		foreach (var url in toObsolete.PublicationUrls
					.ThatAreStreaming()
					.Where(pu => youtubeSync.IsYoutubeUrl(pu.Url)))
		{
			var obsoleteVideo = new YoutubeVideo(
				toObsolete.Id,
				toObsolete.CreateTimestamp,
				url.Url ?? "",
				url.DisplayName,
				toObsolete.Title,
				wikiPage!,
				toObsolete.System!.Code,
				toObsolete.Authors
					.OrderBy(pa => pa.Ordinal)
					.Select(pa => pa.Author!.UserName),
				obsoletingPublicationId);

			await youtubeSync.SyncYouTubeVideo(obsoleteVideo);
		}

		return true;
	}

	public async Task<DateTime?> ExceededSubmissionLimit(int userId)
	{
		var subs = await db.Submissions
			.Where(s => s.SubmitterId == userId
				&& s.CreateTimestamp > DateTime.UtcNow.AddDays(-settings.SubmissionRate.Days))
			.Select(s => s.CreateTimestamp)
			.ToListAsync();

		if (subs.Count >= settings.SubmissionRate.Submissions)
		{
			return subs.Min().AddDays(settings.SubmissionRate.Days);
		}

		return null;
	}

	public async Task<int> GetSubmissionCount(int userId)
		=> await db.Submissions.CountAsync(s => s.SubmitterId == userId);
}

public interface ISubmissionDisplay
{
	SubmissionStatus Status { get; }
	DateTime Date { get; }
}

public record DeleteSubmissionResult(
	DeleteSubmissionResult.DeleteStatus Status,
	string SubmissionTitle,
	string ErrorMessage)
{
	public enum DeleteStatus { Success, NotFound, NotAllowed }

	public bool True => Status == DeleteStatus.Success;

	internal static DeleteSubmissionResult NotFound() => new(DeleteStatus.NotFound, "", "");

	internal static DeleteSubmissionResult IsPublished(string submissionTitle) => new(
		DeleteStatus.NotAllowed,
		submissionTitle,
		"Cannot delete a submission that is published");

	internal static DeleteSubmissionResult Success(string submissionTitle)
		=> new(DeleteStatus.Success, submissionTitle, "");
}

public record UnpublishResult(
	UnpublishResult.UnpublishStatus Status,
	string PublicationTitle,
	string ErrorMessage)
{
	public enum UnpublishStatus { Success, NotFound, NotAllowed }

	internal static UnpublishResult NotFound() => new(UnpublishStatus.NotFound, "", "");

	internal static UnpublishResult HasAwards(string publicationTitle) => new(
		UnpublishStatus.NotAllowed,
		publicationTitle,
		"Cannot unpublish a publication that has awards");

	internal static UnpublishResult Success(string publicationTitle)
		=> new(UnpublishStatus.Success, publicationTitle, "");
}
