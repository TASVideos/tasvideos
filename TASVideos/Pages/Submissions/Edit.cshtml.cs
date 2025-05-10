using Microsoft.AspNetCore.Mvc.RazorPages;
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
	IExternalMediaPublisher publisher,
	ITASVideosGrue tasvideosGrue,
	IMovieFormatDeprecator deprecator,
	IQueueService queueService,
	IYoutubeSync youtubeSync,
	IForumService forumService,
	ITopicWatcher topicWatcher,
	IFileService fileService)
	: SubmitPageModelBase(parser, fileService)
{
	private const string FileFieldName = $"{nameof(Submission)}.{nameof(SubmissionEdit.ReplaceMovieFile)}";

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionEdit Submission { get; set; } = new();

	[BindProperty]
	[DoNotTrim]
	public string Markup { get; set; } = "";

	[BindProperty]
	public bool MarkupChanged { get; set; }

	public bool CanDelete { get; set; }
	public ICollection<SubmissionStatus> AvailableStatuses { get; set; } = [];
	public List<SelectListItem> AvailableClasses { get; set; } = [];
	public List<SelectListItem> AvailableRejectionReasons { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var submission = await db.Submissions
			.Where(s => s.Id == Id)
			.ToSubmissionEditModel()
			.SingleOrDefaultAsync();

		if (submission is null)
		{
			return NotFound();
		}

		Submission = submission;

		var userName = User.Name();
		if (!CanEditSubmission(Submission.Submitter, Submission.Authors))
		{
			return AccessDenied();
		}

		var submissionPage = await wikiPages.SubmissionPage(Id);
		if (submissionPage is not null)
		{
			Markup = submissionPage.Markup;
		}

		await PopulateDropdowns();

		AvailableStatuses = queueService.AvailableStatuses(
			Submission.Status,
			User.Permissions(),
			Submission.SubmitDate,
			Submission.Submitter == userName || Submission.Authors.Contains(userName),
			Submission.Judge == userName,
			Submission.Publisher == userName);

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (User.Has(PermissionTo.ReplaceSubmissionMovieFile) && Submission.ReplaceMovieFile is not null)
		{
			Submission.ReplaceMovieFile?.AddModelErrorIfOverSizeLimit(ModelState, User, movieFieldName: FileFieldName);
		}
		else if (!User.Has(PermissionTo.ReplaceSubmissionMovieFile))
		{
			Submission.ReplaceMovieFile = null;
		}

		// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
		Submission.Authors = Submission.Authors.RemoveEmpty();

		var userName = User.Name();

		// TODO: this is bad, an author can null out these values,
		// but if we treat null as no choice, then we have no way to unset these values
		if (!User.Has(PermissionTo.JudgeSubmissions))
		{
			Submission.IntendedPublicationClass = null;
		}
		else if (Submission.IntendedPublicationClass is null &&
			Submission.Status is SubmissionStatus.Accepted or SubmissionStatus.PublicationUnderway)
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.IntendedPublicationClass)}", "A submission can not be accepted without a PublicationClass");
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

		if (Submission.ReplaceMovieFile is not null)
		{
			var (parseResult, movieFileBytes) = await ParseMovieFile(Submission.ReplaceMovieFile);
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

		if (SubmissionHelper.JudgeIsClaiming(submission.Status, Submission.Status))
		{
			submission.Judge = await db.Users.SingleAsync(u => u.UserName == userName);
		}
		else if (SubmissionHelper.JudgeIsUnclaiming(Submission.Status))
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
		var previousStatus = submission.Status;
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

		submission.IntendedClass = Submission.IntendedPublicationClass.HasValue
			? await db.PublicationClasses.FindAsync(Submission.IntendedPublicationClass.Value)
			: null;

		submission.SubmittedGameVersion = Submission.GameVersion;
		submission.GameName = Submission.GameName;
		submission.EmulatorVersion = Submission.Emulator;
		submission.Branch = Submission.Goal;
		submission.RomName = Submission.RomName;
		submission.EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(Submission.EncodeEmbedLink);
		submission.Status = Submission.Status;
		submission.AdditionalAuthors = Submission.ExternalAuthors.NullIfWhitespace();

		if (MarkupChanged)
		{
			var revision = new WikiCreateRequest
			{
				PageName = $"{LinkConstants.SubmissionWikiPage}{Id}",
				Markup = Markup,
				MinorEdit = HttpContext.Request.MinorEdit(),
				RevisionMessage = Submission.RevisionMessage,
				AuthorId = User.GetUserId()
			};
			_ = await wikiPages.Add(revision) ?? throw new InvalidOperationException("Unable to save wiki revision!");
		}

		submission.SubmissionAuthors.Clear();
		submission.SubmissionAuthors.AddRange(await db.Users
			.ToSubmissionAuthors(submission.Id, Submission.Authors)
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

		var formattedTitle = await GetFormattedTitle(previousStatus, submission.Status);
		var separator = !string.IsNullOrEmpty(Submission.RevisionMessage) ? " | " : "";
		await publisher.SendSubmissionEdit(
			Id, formattedTitle, $"{Submission.RevisionMessage}{separator}{submission.Title}", statusHasChanged);

		return RedirectToPage("View", new { Id });
	}

	private async Task<string> GetFormattedTitle(SubmissionStatus previousStatus, SubmissionStatus newStatus)
	{
		if (previousStatus == newStatus)
		{
			return $"[{Id}S]({{0}}) edited by {User.Name()}";
		}

		string statusStr = newStatus.EnumDisplayName();

		if (previousStatus == SubmissionStatus.PublicationUnderway && newStatus == SubmissionStatus.Accepted)
		{
			return $"[{Id}S]({{0}}) unset {SubmissionStatus.PublicationUnderway.EnumDisplayName()} by {User.Name()}";
		}

		if (newStatus.IsJudgeDecision())
		{
			statusStr = statusStr.ToUpper();
		}

		switch (newStatus)
		{
			case SubmissionStatus.Accepted:
				{
					var publicationClass = (await db.PublicationClasses.FindAsync(Submission.IntendedPublicationClass))!.Name;
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
		var submission = await db.Submissions.FindAsync(Id);
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
		public string? RevisionMessage { get; init; }
		public IFormFile? ReplaceMovieFile { get; set; }
		public int? IntendedPublicationClass { get; set; }
		public int? RejectionReason { get; init; }

		[Required]
		public string GameName { get; init; } = "";
		public string? GameVersion { get; init; }
		public string? RomName { get; init; }
		public string? Goal { get; init; }
		public string? Emulator { get; init; }

		[Url]
		public string? EncodeEmbedLink { get; init; }
		public List<string> Authors { get; set; } = [];
		public string? Submitter { get; init; }
		public DateTime SubmitDate { get; init; }
		public SubmissionStatus Status { get; init; }
		public string? Judge { get; init; }
		public string? Publisher { get; init; }
		public string? ExternalAuthors { get; init; }
		public string Title { get; init; } = "";
	}
}
