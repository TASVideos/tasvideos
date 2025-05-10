using Microsoft.AspNetCore.StaticFiles;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.PublishMovies)]
public class PublishModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	IMediaFileUploader uploader,
	ITASVideoAgent tasVideoAgent,
	IUserManager userManager,
	IFileService fileService,
	IYoutubeSync youtubeSync,
	IQueueService queueService,
	IWebHostEnvironment env)
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

		int? publicationToObsolete = null;
		if (Submission.MovieToObsolete.HasValue)
		{
			publicationToObsolete = (await db.Publications
				.SingleOrDefaultAsync(p => p.Id == Submission.MovieToObsolete.Value))?.Id;
			if (publicationToObsolete is null)
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.MovieToObsolete)}", "Publication does not exist");
				await PopulateDropdowns();
				return Page();
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
			.Include(s => s.IntendedClass)
			.SingleOrDefaultAsync(s => s.Id == Id);

		if (submission is null || !submission.CanPublish())
		{
			return NotFound();
		}

		var movieFileName = Submission.MovieFilename + "." + Submission.MovieExtension;
		if (await db.Publications.AnyAsync(p => p.MovieFileName == movieFileName))
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.MovieFilename)}", $"{nameof(Submission.MovieFilename)} already exists.");
			await PopulateDropdowns();
			return Page();
		}

		using var dbTransaction = await db.Database.BeginTransactionAsync();

		var publication = new Publication
		{
			PublicationClassId = submission.IntendedClass!.Id,
			SystemId = submission.System!.Id,
			SystemFrameRateId = submission.SystemFrameRate!.Id,
			GameId = submission.Game!.Id,
			GameVersionId = submission.GameVersion!.Id,
			EmulatorVersion = submission.EmulatorVersion,
			Frames = submission.Frames,
			RerecordCount = submission.RerecordCount,
			MovieFileName = movieFileName,
			AdditionalAuthors = submission.AdditionalAuthors,
			Submission = submission,
			MovieFile = await fileService.CopyZip(submission.MovieFile, movieFileName),
			GameGoalId = submission.GameGoalId
		};

		publication.PublicationUrls.AddStreaming(Submission.OnlineWatchingUrl, "");
		if (!string.IsNullOrWhiteSpace(Submission.MirrorSiteUrl))
		{
			publication.PublicationUrls.AddMirror(Submission.MirrorSiteUrl);
		}

		if (!string.IsNullOrWhiteSpace(Submission.AlternateOnlineWatchingUrl))
		{
			publication.PublicationUrls.AddStreaming(Submission.AlternateOnlineWatchingUrl, Submission.AlternateOnlineWatchUrlName);
		}

		publication.Authors.CopyFromSubmission(submission.SubmissionAuthors);
		publication.PublicationFlags.AddFlags(Submission.SelectedFlags);
		publication.PublicationTags.AddTags(Submission.SelectedTags);

		db.Publications.Add(publication);

		await db.SaveChangesAsync(); // Need an ID for the Title
		publication.GenerateTitle();

		await uploader.UploadScreenshot(publication.Id, Submission.Screenshot!, Submission.ScreenshotDescription);

		// Create a wiki page corresponding to this publication
		var wikiPage = GenerateWiki(publication.Id, Submission.MovieDescription, User.GetUserId());
		var addedWikiPage = await wikiPages.Add(wikiPage);

		submission.Status = SubmissionStatus.Published;
		db.SubmissionStatusHistory.Add(Id, SubmissionStatus.Published);

		if (publicationToObsolete.HasValue)
		{
			await queueService.ObsoleteWith(publicationToObsolete.Value, publication.Id);
		}

		await userManager.AssignAutoAssignableRolesByPublication(publication.Authors.Select(pa => pa.UserId), publication.Title);
		await tasVideoAgent.PostSubmissionPublished(Id, publication.Id);
		await dbTransaction.CommitAsync();

		var screenshotFilePath = publication.Files.Select(f => f.Path).FirstOrDefault();
		byte[]? screenshotFile = null;
		string? screenshotMimeType = null;
		if (!string.IsNullOrEmpty(screenshotFilePath))
		{
			new FileExtensionContentTypeProvider().TryGetContentType(screenshotFilePath, out screenshotMimeType);
			try
			{
				screenshotFile = await System.IO.File.ReadAllBytesAsync(Path.Combine(env.WebRootPath, "media", screenshotFilePath));
			}
			catch
			{
			}
		}

		await publisher.AnnounceNewPublication(publication, screenshotFile, screenshotMimeType);

		if (youtubeSync.IsYoutubeUrl(Submission.OnlineWatchingUrl))
		{
			var video = new YoutubeVideo(
				publication.Id,
				publication.CreateTimestamp,
				Submission.OnlineWatchingUrl,
				"",
				publication.Title,
				addedWikiPage!,
				submission.System.Code,
				publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				null);
			await youtubeSync.SyncYouTubeVideo(video);
		}

		if (youtubeSync.IsYoutubeUrl(Submission.AlternateOnlineWatchingUrl))
		{
			var video = new YoutubeVideo(
				publication.Id,
				publication.CreateTimestamp,
				Submission.AlternateOnlineWatchingUrl ?? "",
				Submission.AlternateOnlineWatchUrlName,
				publication.Title,
				addedWikiPage!,
				submission.System.Code,
				publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				null);
			await youtubeSync.SyncYouTubeVideo(video);
		}

		return BaseRedirect($"/{publication.Id}M");
	}

	public async Task<IActionResult> OnGetObsoletePublication(int publicationId)
	{
		var pub = await db.Publications
			.Where(p => p.Id == publicationId)
			.Select(p => new ObsoletePublicationResult(p.Title, p.PublicationTags.Select(pt => pt.TagId).ToList()))
			.SingleOrDefaultAsync();

		if (pub is null)
		{
			return BadRequest($"Unable to find publication with an id of {publicationId}");
		}

		var page = await wikiPages.PublicationPage(publicationId);
		pub.Markup = page!.Markup;

		return Json(pub);
	}

	private static WikiCreateRequest GenerateWiki(int publicationId, string markup, int userId) => new()
	{
		RevisionMessage = $"Auto-generated from Movie #{publicationId}",
		PageName = WikiHelper.ToPublicationWikiPageName(publicationId),
		Markup = markup,
		AuthorId = userId
	};

	private async Task PopulateDropdowns()
	{
		AvailableFlags = await db.Flags.ToDropDownList(User.Permissions());
		AvailableTags = await db.Tags.ToDropdownList();
	}

	private record ObsoletePublicationResult(string Title, List<int> Tags)
	{
		public string Markup { get; set; } = "";
	}

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
