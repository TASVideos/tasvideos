using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.SubmitMovies)]
public class SubmitModel(
	IUserManager userManager,
	IMovieFormatDeprecator deprecator,
	IQueueService queueService,
	IWikiPages wikiPages,
	IExternalMediaPublisher externalMediaPublisher)
	: SubmitPageModelBase
{
	private const string FileFieldName = $"{nameof(MovieFile)}";

	[BindProperty]
	[StringLength(20)]
	public string? GameVersion { get; init; }

	[BindProperty]
	[StringLength(100)]
	public string GameName { get; init; } = "";

	[BindProperty]
	[StringLength(50)]
	public string? GoalName { get; init; }

	[BindProperty]
	[StringLength(100)]
	public string RomName { get; init; } = "";

	[BindProperty]
	[StringLength(50)]
	public string? Emulator { get; init; }

	[BindProperty]
	[Url]
	public string? EncodeEmbeddedLink { get; init; }

	[BindProperty]
	[MinLength(1)]
	public IList<string> Authors { get; set; } = [];

	[BindProperty]
	public string? ExternalAuthors { get; init; }

	[BindProperty]
	[DoNotTrim]
	public string Markup { get; init; } = "";

	[BindProperty]
	[Required]
	public IFormFile? MovieFile { get; init; }

	[BindProperty]
	[MustBeTrue(ErrorMessage = "You must read and follow the instructions.")]
	public bool AgreeToInstructions { get; init; }

	[BindProperty]
	[MustBeTrue(ErrorMessage = "You must agree to the license.")]
	public bool AgreeToLicense { get; init; }

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var nextWindow = await queueService.ExceededSubmissionLimit(User.GetUserId());
		if (nextWindow is not null)
		{
			return RedirectToPage("ExceededLimit", new { NextWindow = nextWindow.Value });
		}

		Authors = [User.Name()];
		BackupSubmissionDeterminator = (await queueService.GetSubmissionCount(User.GetUserId())).ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var nextWindow = await queueService.ExceededSubmissionLimit(User.GetUserId());
		if (nextWindow is not null)
		{
			return RedirectToPage("ExceededLimit", new { NextWindow = nextWindow.Value });
		}

		await ValidateModel();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var (parseResult, movieFileBytes) = await queueService.ParseMovieFileOrZip(MovieFile!);
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

		var request = new SubmitRequest(
			GameName,
			RomName,
			GameVersion,
			GoalName,
			Emulator,
			EncodeEmbeddedLink,
			Authors,
			ExternalAuthors,
			Markup,
			movieFileBytes,
			parseResult,
			await userManager.GetRequiredUser(User));

		var result = await queueService.Submit(request);
		if (result.Success)
		{
			await externalMediaPublisher.AnnounceNewSubmission(result.Id, result.Title, result.Screenshot, result.Screenshot is not null ? "image/jpeg" : null, 480, 360);
			return BaseRedirect($"/{result.Id}S");
		}

		ModelState.AddModelError("", result.ErrorMessage!);
		return Page();
	}

	public async Task<IActionResult> OnGetPrefillText()
	{
		var page = await wikiPages.Page(SystemWiki.SubmissionDefaultMessage);
		return Json(new { text = page?.Markup });
	}

	private async Task ValidateModel()
	{
		Authors = Authors.RemoveEmpty();
		if (!Authors.Any() && string.IsNullOrWhiteSpace(ExternalAuthors))
		{
			ModelState.AddModelError(
				$"{nameof(Authors)}",
				"A submission must have at least one author"); // TODO: need to use the AtLeastOne attribute error message since it will be localized
		}

		MovieFile?.AddModelErrorIfOverSizeLimit(ModelState, User, movieFieldName: FileFieldName);

		foreach (var author in Authors)
		{
			if (!await userManager.Exists(author))
			{
				ModelState.AddModelError($"{nameof(Authors)}", $"Could not find user: {author}");
			}
		}
	}
}
