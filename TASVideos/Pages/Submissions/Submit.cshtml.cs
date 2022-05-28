﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.SubmitMovies)]
public class SubmitModel : BasePageModel
{
	private readonly string _fileFieldName = $"{nameof(Create)}.{nameof(SubmissionCreateModel.MovieFile)}";
	private readonly ApplicationDbContext _db;
	private readonly IWikiPages _wikiPages;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IMovieParser _parser;
	private readonly UserManager _userManager;
	private readonly ITASVideoAgent _tasVideoAgent;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IMovieFormatDeprecator _deprecator;
	private readonly IQueueService _queueService;

	public SubmitModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IWikiPages wikiPages,
		IMovieParser parser,
		UserManager userManager,
		ITASVideoAgent tasVideoAgent,
		IYoutubeSync youtubeSync,
		IMovieFormatDeprecator deprecator,
		IQueueService queueService)
	{
		_db = db;
		_publisher = publisher;
		_wikiPages = wikiPages;
		_parser = parser;
		_userManager = userManager;
		_tasVideoAgent = tasVideoAgent;
		_youtubeSync = youtubeSync;
		_deprecator = deprecator;
		_queueService = queueService;
	}

	[BindProperty]
	public SubmissionCreateModel Create { get; set; } = new();

	public void OnGet()
	{
		Create = new SubmissionCreateModel
		{
			Authors = new List<string> { User.Name() }
		};
	}

	public async Task<IActionResult> OnPost()
	{
		await ValidateModel();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var parseResult = await _parser.ParseZip(Create.MovieFile!.OpenReadStream());

		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			return Page();
		}

		var deprecated = await _deprecator.IsDeprecated("." + parseResult.FileExtension);
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
			EncodeEmbedLink = _youtubeSync.ConvertToEmbedLink(Create.EncodeEmbedLink),
			AdditionalAuthors = Create.AdditionalAuthors
		};

		var error = await _queueService.MapParsedResult(parseResult, submission);
		if (!string.IsNullOrWhiteSpace(error))
		{
			ModelState.AddModelError("", error);
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		submission.MovieFile = await Create.MovieFile.ToBytes();
		submission.Submitter = await _userManager.GetUserAsync(User);

		_db.Submissions.Add(submission);
		await _db.SaveChangesAsync();

		await CreateSubmissionWikiPage(submission);

		_db.SubmissionAuthors.AddRange(await _db.Users
			.Where(u => Create.Authors.Contains(u.UserName))
			.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id,
				Author = u,
				Ordinal = Create.Authors.IndexOf(u.UserName)
			})
			.ToListAsync());

		submission.GenerateTitle();

		submission.TopicId = await _tasVideoAgent.PostSubmissionTopic(submission.Id, submission.Title);
		await _db.SaveChangesAsync();

		await _publisher.AnnounceSubmission(submission.Title, $"{submission.Id}S");

		return BaseRedirect($"/{submission.Id}S");
	}

	public async Task<IActionResult> OnGetPrefillText()
	{
		var page = await _wikiPages.Page("System/SubmissionDefaultMessage");
		return new JsonResult(new { text = page?.Markup });
	}

	private async Task ValidateModel()
	{
		Create.Authors = Create.Authors
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();

		if (!Create.Authors.Any())
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
			if (!await _db.Users.Exists(author))
			{
				ModelState.AddModelError($"{nameof(Create)}.{nameof(SubmissionCreateModel.Authors)}", $"Could not find user: {author}");
			}
		}
	}

	private async Task CreateSubmissionWikiPage(Submission submission)
	{
		var revision = new WikiPage
		{
			PageName = LinkConstants.SubmissionWikiPage + submission.Id,
			RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
			Markup = Create.Markup,
			MinorEdit = false,
			AuthorId = User.GetUserId()
		};
		await _wikiPages.Add(revision);
		submission.WikiContent = revision;
	}
}
