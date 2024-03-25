using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions.Models;

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
	private readonly string _fileFieldName = $"{nameof(Create)}.{nameof(SubmissionCreateModel.MovieFile)}";
	private DateTime _earliestTimestamp;

	[BindProperty]
	public SubmissionCreateModel Create { get; set; } = new();

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task OnGet()
	{
		Create = new SubmissionCreateModel
		{
			Authors = [User.Name()]
		};

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

		var parseResult = await parser.ParseZip(Create.MovieFile!.OpenReadStream());

		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			return Page();
		}

		var deprecated = await deprecator.IsDeprecated("." + parseResult.FileExtension);
		if (deprecated)
		{
			ModelState.AddModelError(_fileFieldName, $".{parseResult.FileExtension} is no longer submittable");
			return Page();
		}

		var submission = new Submission
		{
			SubmittedGameVersion = Create.GameVersion,
			GameName = Create.GameName,
			Branch = Create.Branch,
			RomName = Create.RomName,
			EmulatorVersion = Create.Emulator,
			EncodeEmbedLink = youtubeSync.ConvertToEmbedLink(Create.EncodeEmbedLink),
			AdditionalAuthors = Create.AdditionalAuthors
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

		submission.MovieFile = await Create.MovieFile.ToBytes();
		submission.Submitter = await userManager.GetUserAsync(User);

		db.Submissions.Add(submission);
		await db.SaveChangesAsync();

		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = LinkConstants.SubmissionWikiPage + submission.Id,
			RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
			Markup = Create.Markup,
			MinorEdit = false,
			AuthorId = User.GetUserId()
		});

		db.SubmissionAuthors.AddRange(await db.Users
			.ForUsers(Create.Authors)
			.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = Create.Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		submission.GenerateTitle();

		submission.TopicId = await tasVideoAgent.PostSubmissionTopic(submission.Id, submission.Title);
		await db.SaveChangesAsync();

		await publisher.AnnounceSubmission(submission.Title, $"{submission.Id}S");

		return BaseRedirect($"/{submission.Id}S");
	}

	public async Task<IActionResult> OnGetPrefillText()
	{
		var page = await wikiPages.Page("System/SubmissionDefaultMessage");
		return new JsonResult(new { text = page?.Markup });
	}

	private async Task ValidateModel()
	{
		Create.Authors = Create.Authors
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();

		if (!Create.Authors.Any() && string.IsNullOrWhiteSpace(Create.AdditionalAuthors))
		{
			ModelState.AddModelError(
				$"{nameof(Create)}.{nameof(SubmissionCreateModel.Authors)}",
				"A submission must have at least one author"); // TODO: need to use the AtLeastOne attribute error message since it will be localized
		}

		if (!Create.MovieFile.IsZip())
		{
			ModelState.AddModelError(_fileFieldName, "Not a valid .zip file");
		}

		if (!Create.MovieFile.LessThanMovieSizeLimit())
		{
			ModelState.AddModelError(_fileFieldName, ".zip is too big, are you sure this is a valid movie file?");
		}

		foreach (var author in Create.Authors)
		{
			if (!await db.Users.Exists(author))
			{
				ModelState.AddModelError($"{nameof(Create)}.{nameof(SubmissionCreateModel.Authors)}", $"Could not find user: {author}");
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
