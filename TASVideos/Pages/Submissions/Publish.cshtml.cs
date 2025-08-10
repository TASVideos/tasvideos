using Microsoft.AspNetCore.StaticFiles;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.PublishMovies)]
public class PublishModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	IQueueService queueService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionPublishModel Submission { get; set; } = new();

	[BindProperty]
	[DoNotTrim]
	public string? Markup { get; set; }

	public List<SelectListItem> AvailableTags { get; set; } = [];
	public List<SelectListItem> AvailableFlags { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var submission = await db.Submissions
			.Where(s => s.Id == Id)
			.ToPublishModel()
			.SingleOrDefaultAsync();

		if (submission is null)
		{
			return NotFound();
		}

		if (!submission.CanPublish)
		{
			return AccessDenied();
		}

		Submission = submission;
		Markup = (await wikiPages.SubmissionPage(Id))!.Markup;

		await PopulateDropdowns();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!Submission.Screenshot.IsValidImage())
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.Screenshot)}", "Invalid file type. Must be .png or .jpg");
		}

		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return Page();
		}

		var publishRequest = new PublishSubmissionRequest(
			Id,
			Submission.MovieDescription,
			Submission.MovieFilename,
			Submission.MovieExtension!,
			Submission.OnlineWatchingUrl,
			Submission.AlternateOnlineWatchingUrl,
			Submission.AlternateOnlineWatchUrlName,
			Submission.MirrorSiteUrl,
			Submission.Screenshot!,
			Submission.ScreenshotDescription,
			Submission.SelectedFlags,
			Submission.SelectedTags,
			Submission.MovieToObsolete,
			User.GetUserId());

		var result = await queueService.Publish(publishRequest);

		if (!result.Success)
		{
			if (result.ErrorMessage!.Contains("Movie filename") && result.ErrorMessage.Contains("already exists"))
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.MovieFilename)}", $"{nameof(Submission.MovieFilename)} already exists.");
			}
			else if (result.ErrorMessage.Contains("Publication to obsolete does not exist"))
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.MovieToObsolete)}", "Publication does not exist");
			}
			else if (result.ErrorMessage.Contains("Submission not found or cannot be published"))
			{
				return NotFound();
			}
			else
			{
				ModelState.AddModelError("", result.ErrorMessage);
			}

			await PopulateDropdowns();
			return Page();
		}

		new FileExtensionContentTypeProvider().TryGetContentType(result.ScreenshotFilePath, out var screenshotMimeType);
		await publisher.AnnounceNewPublication(result.PublicationId, result.PublicationTitle, result.ScreenshotBytes, screenshotMimeType);

		return BaseRedirect($"/{result.PublicationId}M");
	}

	public async Task<IActionResult> OnGetObsoletePublication(int publicationId)
	{
		var pub = await queueService.GetObsoletePublicationTags(publicationId);
		return pub is null
			? BadRequest($"Unable to find publication with an id of {publicationId}")
			: Json(pub);
	}

	private async Task PopulateDropdowns()
	{
		AvailableFlags = await db.Flags.ToDropDownList(User.Permissions());
		AvailableTags = await db.Tags.ToDropdownList();
	}

	private record ObsoletePublicationResult(string Title, List<int> Tags, string Markup);

	public class SubmissionPublishModel
	{
		public int? MovieToObsolete { get; init; }

		[DoNotTrim]
		public string MovieDescription { get; init; } = SiteGlobalConstants.DefaultPublicationText;
		public string MovieFilename { get; init; } = "";

		[Url]
		[StringLength(500)]
		public string OnlineWatchingUrl { get; init; } = "";

		[Url]
		[StringLength(500)]
		public string? AlternateOnlineWatchingUrl { get; init; }

		[StringLength(100)]
		public string? AlternateOnlineWatchUrlName { get; init; }

		[Url]
		[StringLength(500)]
		public string? MirrorSiteUrl { get; init; }

		[Required]
		public IFormFile? Screenshot { get; init; }
		public string? ScreenshotDescription { get; init; }
		public string? System { get; init; }
		public string? Region { get; init; }
		public string? Game { get; init; }
		public int GameId { get; init; }
		public string? GameVersion { get; init; }
		public int VersionId { get; init; }
		public string? PublicationClass { get; init; }
		public string? MovieExtension { get; init; }
		public List<int> SelectedFlags { get; init; } = [];
		public List<int> SelectedTags { get; init; } = [];

		// Not used for edit fields
		public string Title { get; init; } = "";
		public int SystemId { get; init; }
		public int? SystemFrameRateId { get; init; }
		public SubmissionStatus Status { get; init; }
		public int? GameGoalId { get; init; }
		public string? EmulatorVersion { get; init; }
		public string? Goal { get; init; }

		public bool CanPublish => SystemId > 0
			&& SystemFrameRateId.HasValue
			&& GameId > 0
			&& VersionId > 0
			&& GameGoalId > 0
			&& !string.IsNullOrEmpty(PublicationClass)
			&& Status == SubmissionStatus.PublicationUnderway;
	}
}
