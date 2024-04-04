using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
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
	IQueueService queueService,
	AppSettings settings)
	: BasePageModel
{
	private const string FileFieldName = $"{nameof(MovieFile)}";
	private DateTime _earliestTimestamp;

	[BindProperty]
	[Display(Name = "Game Version", Description = "Example: USA")]
	[StringLength(20)]
	public string? GameVersion { get; init; }

	[BindProperty]
	[Display(Name = "Game Name", Description = "Example: Mega Man 2")]
	[StringLength(100)]
	public string GameName { get; init; } = "";

	[BindProperty]
	[Display(Name = "Goal Name", Description = "Example: 100% or princess only; any% can usually be omitted")]
	[StringLength(50)]
	public string? Branch { get; init; }

	[BindProperty]
	[Display(Name = "ROM filename", Description = "Example: Mega Man II (U) [!].nes")]
	[StringLength(100)]
	public string RomName { get; init; } = "";

	[BindProperty]
	[Display(Name = "Emulator and version", Description = "Example: BizHawk 2.8.0")]
	[StringLength(50)]
	public string? Emulator { get; init; }

	[BindProperty]
	[Url]
	[Display(Name = "Encode Embedded Link", Description = "Embedded link to a video of your movie, Ex: www.youtube.com/embed/0mregEW6kVU")]
	public string? EncodeEmbedLink { get; init; }

	[BindProperty]
	[Display(Name = "Author(s)")]
	[MinLength(1)]
	public IList<string> Authors { get; set; } = [];

	[BindProperty]
	[Display(Name = "External Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
	public string? AdditionalAuthors { get; init; }

	[BindProperty]
	[DoNotTrim]
	[Display(Name = "Comments and explanations")]
	public string Markup { get; init; } = "";

	[BindProperty]
	[Required]
	[Display(Name = "Movie file", Description = "Your movie packed in a ZIP file (max size: 500k)")]
	public IFormFile? MovieFile { get; init; }

	[BindProperty]
	[MustBeTrue(ErrorMessage = "You must read and follow the instructions.")]
	public bool AgreeToInstructions { get; init; }

	[BindProperty]
	[MustBeTrue(ErrorMessage = "You must agree to the license.")]
	public bool AgreeToLicense { get; init; }

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task OnGet()
	{
		Authors = [User.Name()];

		BackupSubmissionDeterminator = (await db.Submissions
			.Where(s => s.SubmitterId == User.GetUserId())
			.CountAsync())
			.ToString();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!await SubmissionAllowed(User.GetUserId()))
		{
			return RedirectToPage("/Submissions/Submit");
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
			Branch = Branch,
			RomName = RomName,
			EmulatorVersion = Emulator,
			EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(EncodeEmbedLink),
			AdditionalAuthors = AdditionalAuthors
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

		await publisher.AnnounceNewSubmission(submission);

		return BaseRedirect($"/{submission.Id}S");
	}

	public async Task<IActionResult> OnGetPrefillText()
	{
		var page = await wikiPages.Page("System/SubmissionDefaultMessage");
		return new JsonResult(new { text = page?.Markup });
	}

	private async Task ValidateModel()
	{
		Authors = Authors
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();

		if (!Authors.Any() && string.IsNullOrWhiteSpace(AdditionalAuthors))
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

	public string[] Notice() =>
	[
		"Sorry, you can not submit at this time.",
		"We limit submissions to " +
		settings.SubmissionRate.Submissions +
		" in " +
		settings.SubmissionRate.Days +
		" days per user. ",
		"You will be able to submit again on " +
		_earliestTimestamp.AddDays(settings.SubmissionRate.Days)
	];

	public async Task<bool> SubmissionAllowed(int userId)
	{
		var subs = await db.Submissions
			.Where(s => s.Submitter != null
				&& s.SubmitterId == userId
				&& s.CreateTimestamp > DateTime.UtcNow.AddDays(-settings.SubmissionRate.Days))
			.ToListAsync();
		if (subs.Count > 0)
		{
			_earliestTimestamp = subs.Select(s => s.CreateTimestamp).Min();
		}

		return subs.Count < settings.SubmissionRate.Submissions;
	}
}
