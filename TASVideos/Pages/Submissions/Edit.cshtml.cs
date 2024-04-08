using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity.Forum;
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
	IForumService forumService)
	: BasePageModel
{
	private const string FileFieldName = $"{nameof(Submission)}.{nameof(SubmissionEdit.MovieFile)}";

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionEdit Submission { get; set; } = new();

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
		var submissionPage = (await wikiPages.SubmissionPage(Id))!;
		Submission.Markup = submissionPage.Markup;
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
		var subAuthors = Submission.Authors
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();
		Submission.Authors = subAuthors;

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

		var availableStatus = queueService.AvailableStatuses(
			subInfo.CurrentStatus,
			User.Permissions(),
			subInfo.CreateDate,
			subInfo.UserIsAuthorOrSubmitter,
			subInfo.UserIsJudge,
			subInfo.UserIsPublisher);

		if (!availableStatus.Contains(Submission.Status))
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.Status)}", $"Invalid status: {Submission.Status}");
		}

		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			AvailableStatuses = availableStatus;
			return Page();
		}

		// If user can not edit submissions then they must be an author or the original submitter
		if (!User.Has(PermissionTo.EditSubmissions))
		{
			if (!subInfo.UserIsAuthorOrSubmitter)
			{
				return AccessDenied();
			}
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
				return Page();
			}

			var deprecated = await deprecator.IsDeprecated("." + parseResult.FileExtension);
			if (deprecated)
			{
				ModelState.AddModelError(FileFieldName, $".{parseResult.FileExtension} is no longer submittable");
				return Page();
			}

			var error = await queueService.MapParsedResult(parseResult, submission);
			if (!string.IsNullOrWhiteSpace(error))
			{
				ModelState.AddModelError("", error);
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			submission.MovieFile = await Submission.MovieFile.ToBytes();
		}

		// If a judge is claiming the submission
		if (Submission.Status == SubmissionStatus.JudgingUnderWay
			&& submission.Status != SubmissionStatus.JudgingUnderWay)
		{
			submission.Judge = await db.Users.SingleAsync(u => u.UserName == userName);
		}
		else if (submission.Status == SubmissionStatus.JudgingUnderWay // If judge is unclaiming, remove them
			&& Submission.Status == SubmissionStatus.New
			&& submission.Judge is not null)
		{
			submission.Judge = null;
		}

		if (Submission.Status == SubmissionStatus.PublicationUnderway
			&& submission.Status != SubmissionStatus.PublicationUnderway)
		{
			submission.Publisher = await db.Users.SingleAsync(u => u.UserName == userName);
		}
		else if (submission.Status == SubmissionStatus.Accepted // If publisher is unclaiming, remove them
			&& Submission.Status == SubmissionStatus.PublicationUnderway)
		{
			submission.Publisher = null;
		}

		bool statusHasChanged = submission.Status != Submission.Status;
		bool moveTopic = false;
		if (statusHasChanged)
		{
			db.SubmissionStatusHistory.Add(submission.Id, Submission.Status);

			int moveTopicTo = -1;

			if (submission.Topic!.ForumId != SiteGlobalConstants.PlaygroundForumId &&
				Submission.Status == SubmissionStatus.Playground)
			{
				moveTopic = true;
				moveTopicTo = SiteGlobalConstants.PlaygroundForumId;
			}
			else if (submission.Topic.ForumId != SiteGlobalConstants.WorkbenchForumId &&
				Submission.Status is SubmissionStatus.New
					or SubmissionStatus.Delayed
					or SubmissionStatus.NeedsMoreInfo
					or SubmissionStatus.JudgingUnderWay
					or SubmissionStatus.Accepted
					or SubmissionStatus.PublicationUnderway)
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
			Markup = Submission.Markup,
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
				Ordinal = subAuthors.IndexOf(u.UserName)
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
			string title;
			string formattedTitle;
			string separator = !string.IsNullOrEmpty(Submission.RevisionMessage) ? " | " : "";
			if (statusHasChanged)
			{
				string statusStr = Submission.Status.EnumDisplayName();

				// CAPS on a judge decision
				if (Submission.Status is SubmissionStatus.Accepted
					or SubmissionStatus.Rejected
					or SubmissionStatus.Cancelled
					or SubmissionStatus.Delayed
					or SubmissionStatus.NeedsMoreInfo
					or SubmissionStatus.Playground)
				{
					statusStr = statusStr.ToUpper();
				}

				if (Submission.Status == SubmissionStatus.Accepted)
				{
					var publicationClass = (await db.PublicationClasses.SingleAsync(t => t.Id == Submission.PublicationClassId)).Name;

					if (publicationClass != "Standard")
					{
						statusStr += $" to {publicationClass}";
					}
				}
				else if (Submission.Status is SubmissionStatus.NeedsMoreInfo
						or SubmissionStatus.New
						or SubmissionStatus.PublicationUnderway
						or SubmissionStatus.Playground)
				{
					statusStr = "set to " + statusStr;
				}

				title = $"Submission {statusStr} by {userName}";
				formattedTitle = $"[{Id}S]({{0}}) {statusStr} by {userName}";
			}
			else
			{
				title = $"Submission edited by {userName}";
				formattedTitle = $"[{Id}S]({{0}}) edited by {userName}";
			}

			await publisher.SendSubmissionEdit(
				title,
				formattedTitle,
				$"{Submission.RevisionMessage}{separator}{submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
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
		}
		else
		{
			submission.PublisherId = User.GetUserId();
		}

		var result = await ConcurrentSave(db, "", "Unable to claim");
		if (result)
		{
			string statusPrefix = newStatus == SubmissionStatus.JudgingUnderWay ? "" : "set to ";
			await publisher.SendSubmissionEdit(
				$"Submission {statusPrefix}{newStatus.EnumDisplayName()} by {User.Name()}",
				$"[Submission]({{0}}) {statusPrefix}{newStatus.EnumDisplayName()} by {User.Name()}",
				$"{submission.Title}",
				$"{Id}S");
		}

		return RedirectToPage("View", new { Id });
	}

	private async Task PopulateDropdowns()
	{
		CanDelete = User.Has(PermissionTo.DeleteSubmissions)
			&& (await queueService.CanDeleteSubmission(Id)).True;

		AvailableClasses = await db.PublicationClasses
			.ToDropDown()
			.ToListAsync();

		AvailableRejectionReasons = await db.SubmissionRejectionReasons
			.ToDropDown()
			.ToListAsync();
	}

	public class SubmissionEdit
	{
		[DoNotTrim]
		[Display(Name = "Comments and explanations")]
		public string Markup { get; set; } = "";

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
