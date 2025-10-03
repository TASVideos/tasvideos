using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Helpers;
using TASVideos.MovieParsers;
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
	/// Returns whether the user has exceeded the submission limit
	/// </summary>
	/// <returns>Next time the user can submit, if the limit has been exceeded, else null</returns>
	Task<DateTime?> ExceededSubmissionLimit(int userId);

	/// <summary>
	/// Returns the total numbers of submissions the given user has submitted
	/// </summary>
	Task<int> GetSubmissionCount(int userId);

	/// <summary>
	/// Updates a submission with the provided data
	/// </summary>
	/// <returns>The result of the update operation</returns>
	Task<UpdateSubmissionResult> UpdateSubmission(UpdateSubmissionRequest request);

	/// <summary>
	/// Creates a new submission
	/// </summary>
	/// <returns>The submission on success or error message on error</returns>
	Task<SubmitResult> Submit(SubmitRequest request);

	/// <summary>
	/// Publishes a submission by creating a publication with all necessary related data
	/// </summary>
	/// <returns>The publication ID on success or error message on error</returns>
	Task<PublishSubmissionResult> Publish(PublishSubmissionRequest request);

	Task<ObsoletePublicationResult?> GetObsoletePublicationTags(int publicationId);

	/// <summary>
	/// Parses a movie file and returns the parse result along with the movie file bytes
	/// Supports both zip files and individual movie files
	/// </summary>
	Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFileOrZip(IFormFile movieFile);

	/// <summary>
	/// Parses an individual movie file and returns the parse result along with the movie file bytes
	/// Does not support zip files - only individual movie files
	/// </summary>
	Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFile(IFormFile movieFile);

	/// <summary>
	/// Claims a submission for judging by the specified user
	/// </summary>
	/// <param name="submissionId">The ID of the submission to claim</param>
	/// <param name="userId">The ID of the user claiming the submission</param>
	/// <param name="userName">The username of the user claiming the submission</param>
	/// <returns>The result of the claim operation</returns>
	Task<ClaimSubmissionResult> ClaimForJudging(int submissionId, int userId, string userName);

	/// <summary>
	/// Claims a submission for publishing by the specified user
	/// </summary>
	/// <param name="submissionId">The ID of the submission to claim</param>
	/// <param name="userId">The ID of the user claiming the submission</param>
	/// <param name="userName">The username of the user claiming the submission</param>
	/// <returns>The result of the claim operation</returns>
	Task<ClaimSubmissionResult> ClaimForPublishing(int submissionId, int userId, string userName);
}

internal class QueueService(
	AppSettings settings,
	ApplicationDbContext db,
	IYoutubeSync youtubeSync,
	ITASVideoAgent tva,
	IWikiPages wikiPages,
	IMediaFileUploader uploader,
	IFileService fileService,
	IUserManager userManager,
	IMovieParser movieParser,
	IMovieFormatDeprecator deprecator,
	IForumService forumService,
	ITASVideosGrue tasvideosGrue,
	ITopicWatcher topicWatcher)
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
			return [.. Enum.GetValues<SubmissionStatus>().Except([Published])]; // Published status must only be set when being published
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

		return sub.IsPublished
			? DeleteSubmissionResult.IsPublished(sub.Title)
			: DeleteSubmissionResult.Success(sub.Title);
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

	public async Task<UpdateSubmissionResult> UpdateSubmission(UpdateSubmissionRequest request)
	{
		var submission = await db.Submissions
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.GameVersion)
			.Include(s => s.GameGoal)
			.Include(gg => gg.GameGoal)
			.Include(s => s.Topic)
			.Include(s => s.Judge)
			.Include(s => s.Publisher)
			.SingleOrDefaultAsync(s => s.Id == request.SubmissionId);

		if (submission is null)
		{
			return UpdateSubmissionResult.Error("Submission not found");
		}

		if (request.ReplaceMovieFile is not null)
		{
			var (parseResult, movieFileBytes) = await ParseMovieFileOrZip(request.ReplaceMovieFile);
			if (!parseResult.Success)
			{
				return UpdateSubmissionResult.Error("Movie file parsing failed");
			}

			var deprecated = await deprecator.IsDeprecated("." + parseResult.FileExtension);
			if (deprecated)
			{
				return UpdateSubmissionResult.Error($".{parseResult.FileExtension} is no longer submittable");
			}

			var mapResult = await MapParsedResult(parseResult);
			if (mapResult is null)
			{
				return UpdateSubmissionResult.Error($"Unknown system type of {parseResult.SystemCode}");
			}

			submission.MovieStartType = mapResult.MovieStartType;
			submission.Frames = mapResult.Frames;
			submission.RerecordCount = mapResult.RerecordCount;
			submission.MovieExtension = mapResult.MovieExtension;
			submission.System = mapResult.System;
			submission.CycleCount = mapResult.CycleCount;
			submission.Annotations = mapResult.Annotations;
			submission.Warnings = mapResult.Warnings;
			submission.SystemFrameRate = mapResult.SystemFrameRate;

			submission.MovieFile = movieFileBytes;
			submission.SyncedOn = null;
			submission.SyncedByUserId = null;

			if (parseResult.Hashes.Count > 0)
			{
				submission.HashType = parseResult.Hashes.First().Key.ToString();
				submission.Hash = parseResult.Hashes.First().Value;
			}
			else
			{
				submission.HashType = null;
				submission.Hash = null;
			}
		}

		if (SubmissionHelper.JudgeIsClaiming(submission.Status, request.Status))
		{
			submission.Judge = await db.Users.SingleAsync(u => u.UserName == request.UserName);
		}
		else if (SubmissionHelper.JudgeIsUnclaiming(request.Status))
		{
			submission.Judge = null;
		}

		if (SubmissionHelper.PublisherIsClaiming(submission.Status, request.Status))
		{
			submission.Publisher = await db.Users.SingleAsync(u => u.UserName == request.UserName);
		}
		else if (SubmissionHelper.PublisherIsUnclaiming(submission.Status, request.Status))
		{
			submission.Publisher = null;
		}

		bool statusHasChanged = submission.Status != request.Status;
		var previousStatus = submission.Status;
		bool requiresTopicMove = false;
		int? moveTopicToForumId = null;

		if (statusHasChanged)
		{
			db.SubmissionStatusHistory.Add(submission.Id, request.Status);

			if (submission.Topic is not null)
			{
				if (submission.Topic.ForumId != SiteGlobalConstants.PlaygroundForumId
					&& request.Status == Playground)
				{
					requiresTopicMove = true;
					moveTopicToForumId = SiteGlobalConstants.PlaygroundForumId;
				}
				else if (submission.Topic.ForumId != SiteGlobalConstants.WorkbenchForumId
						&& request.Status.IsWorkInProgress())
				{
					requiresTopicMove = true;
					moveTopicToForumId = SiteGlobalConstants.WorkbenchForumId;
				}
			}

			// reject/cancel topic move is handled later with TVG's post
			if (requiresTopicMove && moveTopicToForumId.HasValue && submission.Topic is not null)
			{
				submission.Topic.ForumId = moveTopicToForumId.Value;
				var postsToMove = await db.ForumPosts
					.ForTopic(submission.Topic.Id)
					.ToListAsync();
				foreach (var post in postsToMove)
				{
					post.ForumId = moveTopicToForumId.Value;
				}
			}
		}

		submission.RejectionReasonId = request.Status == Rejected
			? request.RejectionReason
			: null;

		submission.IntendedClass = request.IntendedPublicationClass.HasValue
			? await db.PublicationClasses.FindAsync(request.IntendedPublicationClass.Value)
			: null;

		submission.SubmittedGameVersion = request.GameVersion;
		submission.GameName = request.GameName;
		submission.EmulatorVersion = request.Emulator;
		submission.Branch = request.Goal;
		submission.RomName = request.RomName;
		submission.EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(request.EncodeEmbedLink);
		submission.Status = request.Status;
		submission.AdditionalAuthors = request.ExternalAuthors.NormalizeCsv();

		submission.SubmissionAuthors.Clear();
		submission.SubmissionAuthors.AddRange(await db.Users
			.ToSubmissionAuthors(submission.Id, request.Authors)
			.ToListAsync());

		submission.GenerateTitle();

		if (request.MarkupChanged)
		{
			var revision = new WikiCreateRequest
			{
				PageName = $"{LinkConstants.SubmissionWikiPage}{request.SubmissionId}",
				Markup = request.Markup ?? "",
				MinorEdit = request.MinorEdit,
				RevisionMessage = request.RevisionMessage,
				AuthorId = request.UserId
			};
			_ = await wikiPages.Add(revision) ?? throw new InvalidOperationException("Unable to save wiki revision!");
		}

		await db.SaveChangesAsync();

		var topic = await db.ForumTopics.FindAsync(submission.TopicId);
		if (topic is not null)
		{
			topic.Title = submission.Title;
			await db.SaveChangesAsync();
		}

		if (requiresTopicMove)
		{
			forumService.ClearLatestPostCache();
			forumService.ClearTopicActivityCache();
		}

		if (statusHasChanged && request.Status.IsGrueFood())
		{
			await tasvideosGrue.RejectAndMove(request.SubmissionId);
		}

		return new UpdateSubmissionResult(
			null,
			previousStatus,
			submission.Title);
	}

	public async Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFileOrZip(IFormFile movieFile)
	{
		// Inline implementation of DecompressOrTakeRaw
		var rawFileStream = new MemoryStream();
		await movieFile.CopyToAsync(rawFileStream);

		MemoryStream fileStream;
		try
		{
			rawFileStream.Position = 0;
			using var gzip = new GZipStream(rawFileStream, CompressionMode.Decompress, leaveOpen: true);
			var decompressedFileStream = new MemoryStream();
			await gzip.CopyToAsync(decompressedFileStream);
			await rawFileStream.DisposeAsync();
			decompressedFileStream.Position = 0;
			fileStream = decompressedFileStream;
		}
		catch (InvalidDataException)
		{
			rawFileStream.Position = 0;
			fileStream = rawFileStream;
		}

		byte[] fileBytes = fileStream.ToArray();

		// Inline implementation of IsZip
		bool isZip = movieFile.FileName.EndsWith(".zip")
			&& movieFile.ContentType is "application/x-zip-compressed" or "application/zip";

		var parseResult = isZip
			? await movieParser.ParseZip(fileStream)
			: await movieParser.ParseFile(movieFile.FileName, fileStream);

		byte[] movieFileBytes = isZip
			? fileBytes
			: await fileService.ZipFile(fileBytes, movieFile.FileName);

		return (parseResult, movieFileBytes);
	}

	public async Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFile(IFormFile movieFile)
	{
		var rawFileStream = new MemoryStream();
		await movieFile.CopyToAsync(rawFileStream);

		MemoryStream fileStream;
		try
		{
			rawFileStream.Position = 0;
			using var gzip = new GZipStream(rawFileStream, CompressionMode.Decompress, leaveOpen: true);
			var decompressedFileStream = new MemoryStream();
			await gzip.CopyToAsync(decompressedFileStream);
			await rawFileStream.DisposeAsync();
			decompressedFileStream.Position = 0;
			fileStream = decompressedFileStream;
		}
		catch (InvalidDataException)
		{
			rawFileStream.Position = 0;
			fileStream = rawFileStream;
		}

		// Parse the individual movie file (not a zip)
		var parseResult = await movieParser.ParseFile(movieFile.FileName, fileStream);

		// Get the file bytes for storage
		byte[] movieFileBytes = fileStream.ToArray();

		return (parseResult, movieFileBytes);
	}

	public Task<ClaimSubmissionResult> ClaimForJudging(int submissionId, int userId, string userName)
		=> ClaimSubmission(
			submissionId,
			userId,
			userName,
			requiredStatus: New,
			targetStatus: JudgingUnderWay,
			assignToJudge: true,
			wikiMessage: "Claiming for judging.",
			revisionMessage: "Claimed for judging",
			watchTopic: true);

	public Task<ClaimSubmissionResult> ClaimForPublishing(int submissionId, int userId, string userName)
		=> ClaimSubmission(
			submissionId,
			userId,
			userName,
			requiredStatus: Accepted,
			targetStatus: PublicationUnderway,
			assignToJudge: false,
			wikiMessage: "Processing...",
			revisionMessage: "Claimed for publication",
			watchTopic: false);

	public async Task<SubmitResult> Submit(SubmitRequest request)
	{
		try
		{
			using var dbTransaction = await db.BeginTransactionAsync();

			var mapResult = await MapParsedResult(request.ParseResult);
			if (mapResult is null)
			{
				return new FailedSubmitResult($"Unknown system type of {request.ParseResult.SystemCode}");
			}

			var submission = db.Submissions.Add(new Submission
			{
				SubmittedGameVersion = request.GameVersion,
				GameName = request.GameName,
				Branch = request.GoalName?.Trim('"'),
				RomName = request.RomName,
				EmulatorVersion = request.Emulator,
				EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(request.EncodeEmbeddedLink),
				AdditionalAuthors = request.ExternalAuthors.NormalizeCsv(),
				MovieFile = request.MovieFile,
				Submitter = request.Submitter,
				MovieStartType = mapResult.MovieStartType,
				Frames = mapResult.Frames,
				RerecordCount = mapResult.RerecordCount,
				MovieExtension = mapResult.MovieExtension,
				System = mapResult.System,
				CycleCount = mapResult.CycleCount,
				Annotations = mapResult.Annotations,
				Warnings = mapResult.Warnings,
				SystemFrameRate = mapResult.SystemFrameRate
			}).Entity;

			if (request.ParseResult.Hashes.Count > 0)
			{
				submission.HashType = request.ParseResult.Hashes.First().Key.ToString();
				submission.Hash = request.ParseResult.Hashes.First().Value;
			}

			// Save submission to get ID
			await db.SaveChangesAsync();

			// Create wiki page
			await wikiPages.Add(new WikiCreateRequest
			{
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
				Markup = request.Markup,
				AuthorId = request.Submitter.Id
			});

			// Create submission authors
			db.SubmissionAuthors.AddRange(await db.Users
				.ToSubmissionAuthors(submission.Id, request.Authors)
				.ToListAsync());

			// Generate title and create the forum topic
			submission.GenerateTitle();
			submission.TopicId = await tva.PostSubmissionTopic(submission.Id, submission.Title);
			await db.SaveChangesAsync();

			// Commit transaction
			await dbTransaction.CommitAsync();

			// Handle screenshot download and publisher notification (after transaction commit)
			byte[]? screenshotFile = null;
			if (youtubeSync.IsYoutubeUrl(submission.EncodeEmbedLink))
			{
				try
				{
					var youtubeEmbedImageLink = "https://i.ytimg.com/vi/" + submission.EncodeEmbedLink!.Split('/').Last() + "/hqdefault.jpg";
					using var client = new HttpClient();
					var response = await client.GetAsync(youtubeEmbedImageLink);
					if (response.IsSuccessStatusCode)
					{
						screenshotFile = await response.Content.ReadAsByteArrayAsync();
					}
				}
				catch
				{
					// Ignore screenshot download failures
				}
			}

			return new SubmitResult(null, submission.Id, submission.Title, screenshotFile);
		}
		catch (Exception ex)
		{
			return new FailedSubmitResult(ex.ToString());
		}
	}

	public async Task<PublishSubmissionResult> Publish(PublishSubmissionRequest request)
	{
		var submission = await db.Submissions
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.GameVersion)
			.Include(s => s.GameGoal)
			.Include(gg => gg.GameGoal)
			.Include(s => s.IntendedClass)
			.SingleOrDefaultAsync(s => s.Id == request.SubmissionId);

		if (submission is null || !submission.CanPublish())
		{
			return new FailedPublishSubmissionResult("Submission not found or cannot be published");
		}

		var movieFileName = request.MovieFilename + "." + request.MovieExtension;
		if (await db.Publications.AnyAsync(p => p.MovieFileName == movieFileName))
		{
			return new FailedPublishSubmissionResult($"Movie filename {movieFileName} already exists");
		}

		int? publicationToObsolete = null;
		if (request.MovieToObsolete.HasValue)
		{
			publicationToObsolete = (await db.Publications
				.SingleOrDefaultAsync(p => p.Id == request.MovieToObsolete.Value))?.Id;
			if (publicationToObsolete is null)
			{
				return new FailedPublishSubmissionResult("Publication to obsolete does not exist");
			}
		}

		try
		{
			using var dbTransaction = await db.BeginTransactionAsync();

			var publication = new Publication
			{
				PublicationClassId = submission.IntendedClass!.Id,
				SystemId = submission.System!.Id,
				SystemFrameRateId = submission.SystemFrameRate!.Id,
				GameId = submission.Game!.Id,
				GameVersionId = submission.GameVersion!.Id,
				EmulatorVersion = submission.EmulatorVersion,
				Frames = submission.Frames,
				RerecordCount = submission.RerecordCount,
				MovieFileName = movieFileName,
				AdditionalAuthors = submission.AdditionalAuthors,
				Submission = submission,
				MovieFile = await fileService.CopyZip(submission.MovieFile, movieFileName),
				GameGoalId = submission.GameGoalId
			};

			publication.PublicationUrls.AddStreaming(request.OnlineWatchingUrl, "");
			if (!string.IsNullOrWhiteSpace(request.MirrorSiteUrl))
			{
				publication.PublicationUrls.AddMirror(request.MirrorSiteUrl);
			}

			if (!string.IsNullOrWhiteSpace(request.AlternateOnlineWatchingUrl))
			{
				publication.PublicationUrls.AddStreaming(request.AlternateOnlineWatchingUrl, request.AlternateOnlineWatchUrlName);
			}

			publication.Authors.CopyFromSubmission(submission.SubmissionAuthors);
			publication.PublicationFlags.AddFlags(request.SelectedFlags);
			publication.PublicationTags.AddTags(request.SelectedTags);

			db.Publications.Add(publication);

			await db.SaveChangesAsync(); // Need an ID for the Title
			publication.Title = publication.GenerateTitle();

			var (screenshotPath, screenshotBytes) = await uploader.UploadScreenshot(publication.Id, request.Screenshot, request.ScreenshotDescription);

			var addedWikiPage = await wikiPages.Add(new WikiCreateRequest
			{
				RevisionMessage = $"Auto-generated from Movie #{publication.Id}",
				PageName = WikiHelper.ToPublicationWikiPageName(publication.Id),
				Markup = request.MovieDescription,
				AuthorId = request.UserId
			});

			submission.Status = Published;
			db.SubmissionStatusHistory.Add(request.SubmissionId, Published);

			if (publicationToObsolete.HasValue)
			{
				await ObsoleteWith(publicationToObsolete.Value, publication.Id);
			}

			await userManager.AssignAutoAssignableRolesByPublication(publication.Authors.Select(pa => pa.UserId), publication.Title);
			await tva.PostSubmissionPublished(request.SubmissionId, publication.Id);
			await dbTransaction.CommitAsync();

			if (youtubeSync.IsYoutubeUrl(request.OnlineWatchingUrl))
			{
				var video = new YoutubeVideo(
					publication.Id,
					publication.CreateTimestamp,
					request.OnlineWatchingUrl,
					"",
					publication.Title,
					addedWikiPage!,
					submission.System!.Code,
					publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
					null);
				await youtubeSync.SyncYouTubeVideo(video);
			}

			if (youtubeSync.IsYoutubeUrl(request.AlternateOnlineWatchingUrl))
			{
				var video = new YoutubeVideo(
					publication.Id,
					publication.CreateTimestamp,
					request.AlternateOnlineWatchingUrl ?? "",
					request.AlternateOnlineWatchUrlName,
					publication.Title,
					addedWikiPage!,
					submission.System!.Code,
					publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
					null);
				await youtubeSync.SyncYouTubeVideo(video);
			}

			return new PublishSubmissionResult(null, publication.Id, publication.Title, screenshotPath, screenshotBytes);
		}
		catch (Exception ex)
		{
			return new FailedPublishSubmissionResult(ex.ToString());
		}
	}

	public async Task<ObsoletePublicationResult?> GetObsoletePublicationTags(int publicationId)
	{
		var pub = await db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => new { p.Title, Tags = p.PublicationTags.Select(pt => pt.TagId).ToList() })
			.SingleOrDefaultAsync();

		if (pub is null)
		{
			return null;
		}

		var page = await wikiPages.PublicationPage(publicationId);
		return new ObsoletePublicationResult(pub.Title, pub.Tags, page!.Markup);
	}

	internal async Task<bool> ObsoleteWith(int publicationToObsolete, int obsoletingPublicationId)
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

	internal async Task<ParsedSubmissionData?> MapParsedResult(IParseResult parseResult)
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
			return null;
		}

		var annotations = parseResult.Annotations.CapAndEllipse(3500);
		var warnings = parseResult.Warnings.ToList();
		string? warningsString = null;
		if (warnings.Any())
		{
			warningsString = string.Join(",", warnings).Cap(500);
		}

		GameSystemFrameRate? systemFrameRate;
		if (parseResult.FrameRateOverride.HasValue)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			var frameRate = await db.GameSystemFrameRates
				.ForSystem(system.Id)
				.FirstOrDefaultAsync(sf => sf.FrameRate == parseResult.FrameRateOverride.Value);

			if (frameRate is null)
			{
				frameRate = new GameSystemFrameRate
				{
					System = system,
					FrameRate = parseResult.FrameRateOverride.Value,
					RegionCode = parseResult.Region.ToString().ToUpper()
				};
				db.GameSystemFrameRates.Add(frameRate);
				await db.SaveChangesAsync();
			}

			systemFrameRate = frameRate;
		}
		else
		{
			// SingleOrDefault should work here because the only time there could be more than one for a system and region are formats that return a framerate override
			// Those systems should never hit this code block.  But just in case.
			systemFrameRate = await db.GameSystemFrameRates
				.ForSystem(system.Id)
				.ForRegion(parseResult.Region.ToString().ToUpper())
				.FirstOrDefaultAsync();
		}

		return new ParsedSubmissionData(
			(int)parseResult.StartType,
			parseResult.Frames,
			parseResult.RerecordCount,
			parseResult.FileExtension,
			system,
			parseResult.CycleCount,
			annotations,
			warningsString,
			systemFrameRate);
	}

	private async Task<ClaimSubmissionResult> ClaimSubmission(
		int submissionId,
		int userId,
		string userName,
		SubmissionStatus requiredStatus,
		SubmissionStatus targetStatus,
		bool assignToJudge,
		string wikiMessage,
		string revisionMessage,
		bool watchTopic)
	{
		var submission = await db.Submissions.FindAsync(submissionId);
		if (submission is null)
		{
			return ClaimSubmissionResult.Error("Submission not found");
		}

		if (submission.Status != requiredStatus)
		{
			return ClaimSubmissionResult.Error("Submission can not be claimed");
		}

		var submissionPage = (await wikiPages.SubmissionPage(submissionId))!;
		db.SubmissionStatusHistory.Add(submission.Id, submission.Status);

		submission.Status = targetStatus;
		if (assignToJudge)
		{
			submission.JudgeId = userId;
		}
		else
		{
			submission.PublisherId = userId;
		}

		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = submissionPage.PageName,
			Markup = submissionPage.Markup + $"\n----\n[user:{userName}]: {wikiMessage}",
			RevisionMessage = revisionMessage,
			AuthorId = userId
		});

		if (watchTopic && submission.TopicId.HasValue)
		{
			await topicWatcher.WatchTopic(submission.TopicId.Value, userId, true);
		}

		var result = await db.TrySaveChanges();
		return result.IsSuccess()
			? ClaimSubmissionResult.Successful(submission.Title)
			: ClaimSubmissionResult.Error("Unable to claim");
	}
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

public record SubmitRequest(
	string GameName,
	string RomName,
	string? GameVersion,
	string? GoalName,
	string? Emulator,
	string? EncodeEmbeddedLink,
	IList<string> Authors,
	string? ExternalAuthors,
	string Markup,
	byte[] MovieFile,
	IParseResult ParseResult,
	User Submitter);

public record SubmitResult(string? ErrorMessage, int Id, string Title, byte[]? Screenshot)
{
	public bool Success => ErrorMessage == null;
}

public record FailedSubmitResult(string ErrorMessage) : SubmitResult(ErrorMessage, -1, "", null);

public record PublishSubmissionRequest(
	int SubmissionId,
	string MovieDescription,
	string MovieFilename,
	string MovieExtension,
	string OnlineWatchingUrl,
	string? AlternateOnlineWatchingUrl,
	string? AlternateOnlineWatchUrlName,
	string? MirrorSiteUrl,
	IFormFile Screenshot,
	string? ScreenshotDescription,
	List<int> SelectedFlags,
	List<int> SelectedTags,
	int? MovieToObsolete,
	int UserId);

public record PublishSubmissionResult(string? ErrorMessage, int PublicationId, string PublicationTitle, string ScreenshotFilePath, byte[] ScreenshotBytes)
{
	public bool Success => ErrorMessage == null;
}

public record FailedPublishSubmissionResult(string ErrorMessage) : PublishSubmissionResult(ErrorMessage, -1, "", "", []);

public record ObsoletePublicationResult(string Title, List<int> Tags, string Markup);

public record ParsedSubmissionData(
	int MovieStartType,
	int Frames,
	int RerecordCount,
	string MovieExtension,
	GameSystem System,
	long? CycleCount,
	string? Annotations,
	string? Warnings,
	GameSystemFrameRate? SystemFrameRate);

public record UpdateSubmissionRequest(
	int SubmissionId,
	string UserName,
	IFormFile? ReplaceMovieFile,
	int? IntendedPublicationClass,
	int? RejectionReason,
	string GameName,
	string? GameVersion,
	string? RomName,
	string? Goal,
	string? Emulator,
	string? EncodeEmbedLink,
	List<string> Authors,
	string? ExternalAuthors,
	SubmissionStatus Status,
	bool MarkupChanged,
	string? Markup,
	string? RevisionMessage,
	bool MinorEdit,
	int UserId);

public record UpdateSubmissionResult(
	string? ErrorMessage,
	SubmissionStatus PreviousStatus,
	string SubmissionTitle)
{
	public bool Success => ErrorMessage == null;

	public static UpdateSubmissionResult Error(string message) => new(message, New, "");
}

public record ClaimSubmissionResult(
	bool Success,
	string? ErrorMessage,
	string SubmissionTitle)
{
	public static ClaimSubmissionResult Error(string errorMessage) => new(false, errorMessage, "");
	public static ClaimSubmissionResult Successful(string submissionTitle) => new(true, null, submissionTitle);
}
