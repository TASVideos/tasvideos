using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Services;

// TODO: move me
public interface ISubmissionDisplay
{
	SubmissionStatus Status { get; }
	DateTime Submitted { get; }
}

// TODO: move me
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

// TODO: rename this to QueueService
public interface ISubmissionService
{
	/// <summary>
	/// Returns a list of all available statuses a submission could be set to
	/// Based on the user's permissions, submission status and date, and authors.
	/// </summary>
	IEnumerable<SubmissionStatus> AvailableStatuses(SubmissionStatus currentStatus,
		IEnumerable<PermissionTo> userPermissions,
		DateTime submitDate,
		bool isAuthorOrSubmitter,
		bool isJudge,
		bool isPublisher);

	int HoursRemainingForJudging(ISubmissionDisplay submission);

	/// <summary>
	/// Returns whether or not a publication can be unpublished, does not affect the publication
	/// </summary>
	Task<UnpublishResult> CanUnpublish(int publicationId);

	/// <summary>
	/// Deletes a publication and returns the corresponding submission back to the submission queue
	/// </summary>
	Task<UnpublishResult> Unpublish(int publicationId);
}

internal class SubmissionService : ISubmissionService
{
	private readonly int _minimumHoursBeforeJudgment;
	private readonly ApplicationDbContext _db;
	private readonly IYoutubeSync _youtubeSync;

	public SubmissionService(
		AppSettings settings,
		ApplicationDbContext db,
		IYoutubeSync youtubeSync)
	{
		_minimumHoursBeforeJudgment = settings.MinimumHoursBeforeJudgment;
		_db = db;
		_youtubeSync = youtubeSync;
	}

	public IEnumerable<SubmissionStatus> AvailableStatuses(SubmissionStatus currentStatus,
		IEnumerable<PermissionTo> userPermissions,
		DateTime submitDate,
		bool isAuthorOrSubmitter,
		bool isJudge,
		bool isPublisher)
	{

		// Published submissions can not be changed
		if (currentStatus == Published)
		{
			return new List<SubmissionStatus> { Published };
		}

		var perms = userPermissions.ToList();
		if (perms.Contains(PermissionTo.OverrideSubmissionStatus))
		{
			return Enum.GetValues(typeof(SubmissionStatus))
				.Cast<SubmissionStatus>()
				.Except(new[] { Published }); // Published status must only be set when being published
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
			|| (isJudge || isAuthorOrSubmitter) && currentStatus == Cancelled)
		{
			list.Add(New);
		}

		// A judge can claim a new run, unless they are not author or the submitter
		if (new[] { New, JudgingUnderWay, Delayed, NeedsMoreInfo, Accepted, Rejected, PublicationUnderway, Cancelled }.Contains(currentStatus)
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
		else if ((currentStatus == Accepted || currentStatus == PublicationUnderway)
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

		return list;
	}

	public int HoursRemainingForJudging(ISubmissionDisplay submission)
	{
		if (submission.Status.CanBeJudged())
		{
			var diff = (DateTime.UtcNow - submission.Submitted).TotalHours;
			return _minimumHoursBeforeJudgment - (int)diff;
		}

		return 0;
	}

	public async Task<UnpublishResult> CanUnpublish(int publicationId)
	{
		var pub = await _db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => new
			{
				p.Title,
				HasAwards = p.PublicationAwards.Any()
			})
			.SingleOrDefaultAsync();

		if (pub == null)
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
		var publication = await _db.Publications
			.Include(p => p.PublicationAwards)
			.Include(p => p.Authors)
			.Include(p => p.Files)
			.Include(p => p.PublicationFlags)
			.Include(p => p.PublicationRatings)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationUrls)
			.Include(p => p.Submission)
			.Include(p => p.ObsoletedMovies)
			.SingleOrDefaultAsync(p => p.Id == publicationId);

		if (publication == null)
		{
			return UnpublishResult.NotFound();
		}

		if (publication.PublicationAwards.Any())
		{
			return UnpublishResult.HasAwards(publication.Title);
		}

		var youtubeUrls = publication.PublicationUrls
			.Select(pu => pu.Url)
			.Where(url => _youtubeSync.IsYoutubeUrl(url))
			.ToList();

		// Add to submission status history
		// Reset the submission status
		// TVA post?
		// Youtube sync - set urls to unlisted
		// Youtube sync - if there was an obsoleted movie, sync it
		publication.Authors.Clear();
		publication.Files.Clear();
		publication.PublicationFlags.Clear();
		publication.PublicationRatings.Clear();
		publication.PublicationTags.Clear();

		await _db.SaveChangesAsync();

		return UnpublishResult.Success(publication.Title);
	}
}
