using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Helpers;
using TASVideos.MovieParsers;

namespace TASVideos.Pages.Submissions;

[RequirePermission(true, PermissionTo.SubmitMovies, PermissionTo.EditSubmissions)]
public class EditModel(
	ApplicationDbContext db,
	IMovieParser parser,
	IWikiPages wikiPages,
	ExternalMediaPublisher publisher,
	ITASVideosGrue tasvideosGrue,
	IMovieFormatDeprecator deprecator,
	IQueueService queueService,
	IYoutubeSync youtubeSync,
	IForumService forumService,
	ITopicWatcher topicWatcher)
	: BasePageModel
{
	private const string FileFieldName = $"{nameof(Submission)}.{nameof(SubmissionEdit.MovieFile)}";

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionEdit Submission { get; set; } = new();

	[BindProperty]
	[DoNotTrim]
	[Display(Name = "Comments and explanations")]
	public string Markup { get; set; } = "";

	public bool CanDelete { get; set; }
	public ICollection<SubmissionStatus> AvailableStatuses { get; set; } = [];
	public List<SelectListItem> AvailableClasses { get; set; } = [];
	public List<SelectListItem> AvailableRejectionReasons { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var submission = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionEdit // It is important to use a projection here to avoid querying the file data which not needed and can be slow
			{
				GameName = s.GameName ?? "",
				GameVersion = s.SubmittedGameVersion,
				RomName = s.RomName,
				Goal = s.Branch,
				Emulator = s.EmulatorVersion,
				CreateTimestamp = s.CreateTimestamp,
				Submitter = s.Submitter!.UserName,
				Status = s.Status,
				EncodeEmbedLink = s.EncodeEmbedLink,
				Judge = s.Judge != null ? s.Judge.UserName : "",
				Publisher = s.Publisher != null ? s.Publisher.UserName : "",
				PublicationClassId = s.IntendedClassId,
				RejectionReason = s.RejectionReasonId,
				AdditionalAuthors = s.AdditionalAuthors
			})
			.SingleOrDefaultAsync();

		if (submission is null)
		{
			return NotFound();
		}

		Submission = submission;
		var submissionPage = await wikiPages.SubmissionPage(Id);
		if (submissionPage is not null)
		{
			Markup = submissionPage.Markup;
		}

		Submission.Authors = await db.SubmissionAuthors
			.Where(sa => sa.SubmissionId == Id)
			.OrderBy(sa => sa.Ordinal)
			.Select(sa => sa.Author!.UserName)
			.ToListAsync();

		var userName = User.Name();

		// If user can not edit submissions then they must be an author or the original submitter
		if (!User.Has(PermissionTo.EditSubmissions)
			&& Submission.Submitter != userName
			&& !Submission.Authors.Contains(userName))
		{
				return AccessDenied();
		}

		await PopulateDropdowns();

		AvailableStatuses = queueService.AvailableStatuses(
			Submission.Status,
			User.Permissions(),
			Submission.CreateTimestamp,
			Submission.Submitter == userName || Submission.Authors.Contains(userName),
			Submission.Judge == userName,
			Submission.Publisher == userName);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (User.Has(PermissionTo.ReplaceSubmissionMovieFile) && Submission.MovieFile is not null)
		{
			if (!Submission.MovieFile.IsZip())
			{
				ModelState.AddModelError(FileFieldName, "Not a valid .zip file");
			}

			if (!Submission.MovieFile.LessThanMovieSizeLimit())
			{
				ModelState.AddModelError(FileFieldName, ".zip is too big, are you sure this is a valid movie file?");
			}
		}
		else if (!User.Has(PermissionTo.ReplaceSubmissionMovieFile))
		{
			Submission.MovieFile = null;
		}

		// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
		Submission.Authors = Submission.Authors.RemoveEmpty();

		var userName = User.Name();

		// TODO: this is bad, an author can null out these values,
		// but if we treat null as no choice, then we have no way to unset these values
		if (!User.Has(PermissionTo.JudgeSubmissions))
		{
			Submission.PublicationClassId = null;
		}
		else if (Submission.PublicationClassId is null &&
			Submission.Status is SubmissionStatus.Accepted or SubmissionStatus.PublicationUnderway)
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.PublicationClassId)}", "A submission can not be accepted without a PublicationClass");
		}

		var subInfo = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new
			{
				UserIsJudge = s.Judge != null && s.Judge.UserName == userName,
				UserIsPublisher = s.Publisher != null && s.Publisher.UserName == userName,
				UserIsAuthorOrSubmitter = s.Submitter!.UserName == userName || s.SubmissionAuthors.Any(sa => sa.Author!.UserName == userName),
				CurrentStatus = s.Status,
				CreateDate = s.CreateTimestamp
			})
			.SingleOrDefaultAsync();

		if (subInfo is null)
		{
			return NotFound();
		}

		AvailableStatuses = queueService.AvailableStatuses(
			subInfo.CurrentStatus,
			User.Permissions(),
			subInfo.CreateDate,
			subInfo.UserIsAuthorOrSubmitter,
			subInfo.UserIsJudge,
			subInfo.UserIsPublisher);

		if (!AvailableStatuses.Contains(Submission.Status))
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.Status)}", $"Invalid status: {Submission.Status}");
		}

		if (!ModelState.IsValid)
		{
			return await ReturnWithModelErrors();
		}

		if (!User.Has(PermissionTo.EditSubmissions) && !subInfo.UserIsAuthorOrSubmitter)
		{
			return AccessDenied();
		}

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
			.SingleAsync(s => s.Id == Id);

		if (Submission.MovieFile is not null)
		{
			var parseResult = await parser.ParseZip(Submission.MovieFile.OpenReadStream());
			if (!parseResult.Success)
			{
				ModelState.AddParseErrors(parseResult);
				return await ReturnWithModelErrors();
			}

			var deprecated = await deprecator.IsDeprecated("." + parseResult.FileExtension);
			if (deprecated)
			{
				ModelState.AddModelError(FileFieldName, $".{parseResult.FileExtension} is no longer submittable");
				return await ReturnWithModelErrors();
			}

			var error = await queueService.MapParsedResult(parseResult, submission);
			if (!string.IsNullOrWhiteSpace(error))
			{
				ModelState.AddModelError("", error);
			}

			if (!ModelState.IsValid)
			{
				return await ReturnWithModelErrors();
			}

			submission.MovieFile = await Submission.MovieFile.ToBytes();
		}

		if (SubmissionHelper.JudgeIsClaiming(submission.Status, Submission.Status))
		{
			submission.Judge = await db.Users.SingleAsync(u => u.UserName == userName);
		}
		else if (SubmissionHelper.JudgeIsUnclaiming(submission.Status, Submission.Status))
		{
			submission.Judge = null;
		}

		if (SubmissionHelper.PublisherIsClaiming(submission.Status, Submission.Status))
		{
			submission.Publisher = await db.Users.SingleAsync(u => u.UserName == userName);
		}
		else if (SubmissionHelper.PublisherIsUnclaiming(submission.Status, Submission.Status))
		{
			submission.Publisher = null;
		}

		bool statusHasChanged = submission.Status != Submission.Status;
		bool moveTopic = false;
		if (statusHasChanged)
		{
			db.SubmissionStatusHistory.Add(submission.Id, Submission.Status);

			int moveTopicTo = -1;

			if (submission.Topic!.ForumId != SiteGlobalConstants.PlaygroundForumId
				&& Submission.Status == SubmissionStatus.Playground)
			{
				moveTopic = true;
				moveTopicTo = SiteGlobalConstants.PlaygroundForumId;
			}
			else if (submission.Topic.ForumId != SiteGlobalConstants.WorkbenchForumId
					&& Submission.Status.IsWorkInProgress())
			{
				moveTopic = true;
				moveTopicTo = SiteGlobalConstants.WorkbenchForumId;
			}

			// reject/cancel topic move is handled later with TVG's post
			if (moveTopic)
			{
				submission.Topic.ForumId = moveTopicTo;
				var postsToMove = await db.ForumPosts
					.ForTopic(submission.Topic.Id)
					.ToListAsync();
				foreach (var post in postsToMove)
				{
					post.ForumId = moveTopicTo;
				}
			}
		}

		submission.RejectionReasonId = Submission.Status == SubmissionStatus.Rejected
			? Submission.RejectionReason
			: null;

		submission.IntendedClass = Submission.PublicationClassId.HasValue
			? await db.PublicationClasses.SingleAsync(t => t.Id == Submission.PublicationClassId.Value)
			: null;

		submission.SubmittedGameVersion = Submission.GameVersion;
		submission.GameName = Submission.GameName;
		submission.EmulatorVersion = Submission.Emulator;
		submission.Branch = Submission.Goal;
		submission.RomName = Submission.RomName;
		submission.EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(Submission.EncodeEmbedLink);
		submission.Status = Submission.Status;
		submission.AdditionalAuthors = Submission.AdditionalAuthors.NullIfWhitespace();

		var revision = new WikiCreateRequest
		{
			PageName = $"{LinkConstants.SubmissionWikiPage}{Id}",
			Markup = Markup,
			MinorEdit = Submission.MinorEdit,
			RevisionMessage = Submission.RevisionMessage,
			AuthorId = User.GetUserId()
		};
		_ = await wikiPages.Add(revision) ?? throw new InvalidOperationException("Unable to save wiki revision!");
		submission.SubmissionAuthors.Clear();
		submission.SubmissionAuthors.AddRange(await db.Users
			.ForUsers(Submission.Authors)
			.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = Submission.Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		submission.GenerateTitle();
		await db.SaveChangesAsync();

		var topic = await db.ForumTopics.FirstOrDefaultAsync(t => t.Id == submission.TopicId);
		if (topic is not null)
		{
			topic.Title = submission.Title;
			await db.SaveChangesAsync();
		}

		if (moveTopic)
		{
			forumService.ClearLatestPostCache();
			forumService.ClearTopicActivityCache();
		}

		if (submission.Status is SubmissionStatus.Rejected or SubmissionStatus.Cancelled
			&& statusHasChanged)
		{
			await tasvideosGrue.RejectAndMove(submission.Id);
		}

		if (!Submission.MinorEdit || statusHasChanged) // always publish submission status changes to media
		{
			var formattedTitle = await GetFormattedTitle(statusHasChanged);
			var separator = !string.IsNullOrEmpty(Submission.RevisionMessage) ? " | " : "";
			await publisher.SendSubmissionEdit(
				Id, formattedTitle, $"{Submission.RevisionMessage}{separator}{submission.Title}");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task<string> GetFormattedTitle(bool statusHasChanged)
	{
		if (!statusHasChanged)
		{
			return $"[{Id}S]({{0}}) edited by {User.Name()}";
		}

		string statusStr = Submission.Status.EnumDisplayName();

		if (Submission.Status.IsJudgeDecision())
		{
			statusStr = statusStr.ToUpper();
		}

		switch (Submission.Status)
		{
			case SubmissionStatus.Accepted:
			{
				var publicationClass = (await db.PublicationClasses.SingleAsync(t => t.Id == Submission.PublicationClassId)).Name;
				if (publicationClass != "Standard")
				{
					statusStr += $" to {publicationClass}";
				}

				break;
			}

			case SubmissionStatus.NeedsMoreInfo
				or SubmissionStatus.New
				or SubmissionStatus.PublicationUnderway
				or SubmissionStatus.Playground:
				statusStr = "set to " + statusStr;
				break;
		}

		return $"[{Id}S]({{0}}) {statusStr} by {User.Name()}";
	}

	public async Task<IActionResult> OnGetClaimForJudging()
	{
		if (!User.Has(PermissionTo.JudgeSubmissions))
		{
			return AccessDenied();
		}

		return await Claim(SubmissionStatus.New, SubmissionStatus.JudgingUnderWay, "judging", "Claiming for judging.", true);
	}

	public async Task<IActionResult> OnGetClaimForPublishing()
	{
		if (!User.Has(PermissionTo.PublishMovies))
		{
			return AccessDenied();
		}

		return await Claim(SubmissionStatus.Accepted, SubmissionStatus.PublicationUnderway, "publication", "Processing...", false);
	}

	private async Task<IActionResult> Claim(SubmissionStatus requiredStatus, SubmissionStatus newStatus, string action, string message, bool isJudge)
	{
		var submission = await db.Submissions.SingleOrDefaultAsync(s => s.Id == Id);

		if (submission is null)
		{
			return NotFound();
		}

		if (submission.Status != requiredStatus)
		{
			return BadRequest("Submission can not be claimed");
		}

		var submissionPage = (await wikiPages.SubmissionPage(Id))!;
		db.SubmissionStatusHistory.Add(submission.Id, Submission.Status);

		submission.Status = newStatus;
		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = submissionPage.PageName,
			Markup = submissionPage.Markup + $"\n----\n[user:{User.Name()}]: {message}",
			RevisionMessage = $"Claimed for {action}",
			AuthorId = User.GetUserId()
		});

		if (isJudge)
		{
			submission.JudgeId = User.GetUserId();
			if (submission.TopicId.HasValue)
			{
				await topicWatcher.WatchTopic(submission.TopicId.Value, User.GetUserId(), true);
			}
		}
		else
		{
			submission.PublisherId = User.GetUserId();
		}

		var result = await db.TrySaveChanges();
		SetMessage(result, "", "Unable to claim");
		if (result.IsSuccess())
		{
			await publisher.SendSubmissionEdit(Id, $"[Submission]({{0}}) {newStatus.EnumDisplayName()} by {User.Name()}", $"{submission.Title}");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task<PageResult> ReturnWithModelErrors()
	{
		await PopulateDropdowns();
		return Page();
	}

	private async Task PopulateDropdowns()
	{
		CanDelete = User.Has(PermissionTo.DeleteSubmissions)
			&& (await queueService.CanDeleteSubmission(Id)).True;

		AvailableClasses = await db.PublicationClasses.ToDropDownList();
		AvailableRejectionReasons = await db.SubmissionRejectionReasons.ToDropDownList();
	}

	public class SubmissionEdit
	{
		[StringLength(1000)]
		[Display(Name = "Revision Message")]
		public string? RevisionMessage { get; init; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; init; }

		[Display(Name = "Replace Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile? MovieFile { get; set; }

		[Display(Name = "Intended Publication Class")]
		public int? PublicationClassId { get; set; }

		[Display(Name = "Reason")]
		public int? RejectionReason { get; init; }

		[Display(Name = "Game name")]
		[Required]
		public string GameName { get; init; } = "";

		[Display(Name = "Game Version")]
		public string? GameVersion { get; init; }

		[Display(Name = "ROM filename")]
		public string? RomName { get; init; }
		public string? Goal { get; init; }
		public string? Emulator { get; init; }

		[Url]
		[Display(Name = "Encode Embed Link")]
		public string? EncodeEmbedLink { get; init; }

		[Display(Name = "Author(s)")]
		public List<string> Authors { get; set; } = [];
		public string? Submitter { get; init; }

		[Display(Name = "Submit Date")]
		public DateTime CreateTimestamp { get; init; }
		public SubmissionStatus Status { get; init; }
		public string? Judge { get; init; }
		public string? Publisher { get; init; }

		[Display(Name = "External Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
		public string? AdditionalAuthors { get; init; }
	}
}
