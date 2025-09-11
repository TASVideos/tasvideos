using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Submissions;

[RequirePermission(true, PermissionTo.SubmitMovies, PermissionTo.EditSubmissions)]
public class EditModel(
	ApplicationDbContext db,
	IWikiPages wikiPages,
	IExternalMediaPublisher publisher,
	IQueueService queueService)
	: SubmitPageModelBase
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

		var userName = User.Name();
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
			return await ReturnWithModelErrors();
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

		if (!User.Has(PermissionTo.EditSubmissions) && !subInfo.UserIsAuthorOrSubmitter)
		{
			return AccessDenied();
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
			return await ReturnWithModelErrors();
		}

		var updateRequest = new UpdateSubmissionRequest(
			Id,
			userName,
			Submission.ReplaceMovieFile,
			Submission.IntendedPublicationClass,
			Submission.RejectionReason,
			Submission.GameName,
			Submission.GameVersion,
			Submission.RomName,
			Submission.Goal,
			Submission.Emulator,
			Submission.EncodeEmbedLink,
			Submission.Authors,
			Submission.ExternalAuthors,
			Submission.Status,
			MarkupChanged,
			Markup,
			Submission.RevisionMessage,
			HttpContext.Request.MinorEdit(),
			User.GetUserId());

		var updateResult = await queueService.UpdateSubmission(updateRequest);
		if (!updateResult.Success)
		{
			ModelState.AddModelError("", updateResult.ErrorMessage!);
			return await ReturnWithModelErrors();
		}

		var formattedTitle = await GetFormattedTitle(updateResult.PreviousStatus, Submission.Status);
		var separator = !string.IsNullOrEmpty(Submission.RevisionMessage) ? " | " : "";
		await publisher.SendSubmissionEdit(
			Id, formattedTitle, $"{Submission.RevisionMessage}{separator}{updateResult.SubmissionTitle}",  updateResult.PreviousStatus != Submission.Status);

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

		var result = await queueService.ClaimForJudging(Id, User.GetUserId(), User.Name());
		SetMessage(result.Success, "", result.ErrorMessage ?? "Unable to claim");

		if (result.Success)
		{
			await publisher.SendSubmissionEdit(Id, $"[Submission]({{0}}) {SubmissionStatus.JudgingUnderWay.EnumDisplayName()} by {User.Name()}", result.SubmissionTitle);
		}

		return RedirectToPage("View", new { Id });
	}

	public async Task<IActionResult> OnGetClaimForPublishing()
	{
		if (!User.Has(PermissionTo.PublishMovies))
		{
			return AccessDenied();
		}

		var result = await queueService.ClaimForPublishing(Id, User.GetUserId(), User.Name());
		SetMessage(result.Success, "", result.ErrorMessage ?? "Unable to claim");

		if (result.Success)
		{
			await publisher.SendSubmissionEdit(Id, $"[Submission]({{0}}) {SubmissionStatus.PublicationUnderway.EnumDisplayName()} by {User.Name()}", result.SubmissionTitle);
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
