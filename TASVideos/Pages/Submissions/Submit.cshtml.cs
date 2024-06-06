using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.MovieParsers;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.SubmitMovies)]
public class SubmitModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	IMovieParser parser,
	UserManager userManager,
	ITASVideoAgent tasVideoAgent,
	IYoutubeSync youtubeSync,
	IMovieFormatDeprecator deprecator,
	IQueueService queueService)
	: BasePageModel
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
		BackupSubmissionDeterminator = (await db.Submissions
			.CountAsync(s => s.SubmitterId == User.GetUserId()))
			.ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		await using var dbTransaction = await db.Database.BeginTransactionAsync();
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

		var parseResult = await parser.ParseZip(MovieFile!.OpenReadStream());
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

		var submission = new Submission
		{
			SubmittedGameVersion = GameVersion,
			GameName = GameName,
			Branch = GoalName,
			RomName = RomName,
			EmulatorVersion = Emulator,
			EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(EncodeEmbeddedLink),
			AdditionalAuthors = ExternalAuthors
		};

		var error = await queueService.MapParsedResult(parseResult, submission);
		if (!string.IsNullOrWhiteSpace(error))
		{
			ModelState.AddModelError("", error);
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		submission.MovieFile = await MovieFile.ToBytes();
		submission.Submitter = await userManager.GetUserAsync(User);

		db.Submissions.Add(submission);
		await db.SaveChangesAsync();

		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = LinkConstants.SubmissionWikiPage + submission.Id,
			RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
			Markup = Markup,
			MinorEdit = false,
			AuthorId = User.GetUserId()
		});

		db.SubmissionAuthors.AddRange(await db.Users
			.ForUsers(Authors)
			.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		submission.GenerateTitle();

		submission.TopicId = await tasVideoAgent.PostSubmissionTopic(submission.Id, submission.Title);
		await db.SaveChangesAsync();
		await dbTransaction.CommitAsync();
		await publisher.AnnounceNewSubmission(submission);

		return BaseRedirect($"/{submission.Id}S");
	}

	public async Task<IActionResult> OnGetPrefillText()
	{
		var page = await wikiPages.Page("System/SubmissionDefaultMessage");
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

		if (!MovieFile.IsZip())
		{
			ModelState.AddModelError(FileFieldName, "Not a valid .zip file");
		}

		if (!MovieFile.LessThanMovieSizeLimit())
		{
			ModelState.AddModelError(FileFieldName, ".zip is too big, are you sure this is a valid movie file?");
		}

		foreach (var author in Authors)
		{
			if (!await db.Users.Exists(author))
			{
				ModelState.AddModelError($"{nameof(Authors)}", $"Could not find user: {author}");
			}
		}
	}
}
