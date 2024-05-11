using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.PublishMovies)]
public class PublishModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IWikiPages wikiPages,
	IMediaFileUploader uploader,
	ITASVideoAgent tasVideoAgent,
	UserManager userManager,
	IFileService fileService,
	IYoutubeSync youtubeSync,
	IQueueService queueService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionPublishModel Submission { get; set; } = new();

	[BindProperty]
	[DoNotTrim]
	[Display(Name = "Submission description (for quoting, reference, etc)")]
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

		var movieFileName = Submission.MovieFileName + "." + Submission.MovieExtension;
		if (await db.Publications.AnyAsync(p => p.MovieFileName == movieFileName))
		{
			ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.MovieFileName)}", $"{nameof(Submission.MovieFileName)} already exists.");
			await PopulateDropdowns();
			return Page();
		}

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

		publication.PublicationUrls.AddStreaming(Submission.OnlineWatchingUrl, Submission.OnlineWatchUrlName);
		publication.PublicationUrls.AddMirror(Submission.MirrorSiteUrl);
		publication.Authors.CopyFromSubmission(submission.SubmissionAuthors);
		publication.PublicationFlags.AddFlags(Submission.SelectedFlags);
		publication.PublicationTags.AddTags(Submission.SelectedTags);

		db.Publications.Add(publication);

		await db.SaveChangesAsync(); // Need an Id for the Title
		publication.GenerateTitle();

		await uploader.UploadScreenshot(publication.Id, Submission.Screenshot!, Submission.ScreenshotDescription);

		// Create a wiki page corresponding to this publication
		var wikiPage = GenerateWiki(publication.Id, Submission.MovieMarkup, User.GetUserId());
		var addedWikiPage = await wikiPages.Add(wikiPage);

		submission.Status = SubmissionStatus.Published;
		db.SubmissionStatusHistory.Add(Id, SubmissionStatus.Published);

		if (publicationToObsolete.HasValue)
		{
			await queueService.ObsoleteWith(publicationToObsolete.Value, publication.Id);
		}

		await userManager.AssignAutoAssignableRolesByPublication(publication.Authors.Select(pa => pa.UserId), publication.Title);
		await tasVideoAgent.PostSubmissionPublished(Id, publication.Id);
		await publisher.AnnounceNewPublication(publication);

		if (youtubeSync.IsYoutubeUrl(Submission.OnlineWatchingUrl))
		{
			var video = new YoutubeVideo(
				publication.Id,
				publication.CreateTimestamp,
				Submission.OnlineWatchingUrl,
				Submission.OnlineWatchUrlName,
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

		return new JsonResult(pub);
	}

	private static WikiCreateRequest GenerateWiki(int publicationId, string markup, int userId)
	{
		return new WikiCreateRequest
		{
			RevisionMessage = $"Auto-generated from Movie #{publicationId}",
			PageName = WikiHelper.ToPublicationWikiPageName(publicationId),
			MinorEdit = false,
			Markup = markup,
			AuthorId = userId
		};
	}

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
		[Display(Name = "Select movie to be obsoleted")]
		public int? MovieToObsolete { get; init; }

		[DoNotTrim]
		[Display(Name = "Movie description")]
		public string MovieMarkup { get; init; } = SiteGlobalConstants.DefaultPublicationText;

		[Display(Name = "Movie Filename", Description = "Please follow the convention: xxxv#-yyy where xxx is author name, # is version and yyy is game name. Special characters such as \"&\" and \"/\" and \".\" and spaces must not occur in the filename.")]
		public string MovieFileName { get; init; } = "";

		[Url]
		[Display(Name = "Online-watching URL")]
		[StringLength(500)]
		public string OnlineWatchingUrl { get; init; } = "";

		[StringLength(100)]
		[Display(Name = "Online-watching URL Display Name (Optional)")]
		public string? OnlineWatchUrlName { get; init; }

		[Url]
		[Display(Name = "Mirror site URL")]
		[StringLength(500)]
		public string MirrorSiteUrl { get; init; } = "";

		[Required]
		[Display(Name = "Screenshot", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile? Screenshot { get; init; }

		[Display(Name = "Description", Description = "Caption, describe what happens in the screenshot")]
		public string? ScreenshotDescription { get; init; }

		[Display(Name = "System")]
		public string? SystemCode { get; init; }

		[Display(Name = "Region")]
		public string? SystemRegion { get; init; }
		public string? Game { get; init; }
		public int GameId { get; init; }

		[Display(Name = "Game Version")]
		public string? GameVersion { get; init; }
		public int VersionId { get; init; }
		public string? PublicationClass { get; init; }
		public string? MovieExtension { get; init; }

		[Display(Name = "Selected Flags")]
		public List<int> SelectedFlags { get; init; } = [];

		[Display(Name = "Selected Tags")]
		public List<int> SelectedTags { get; init; } = [];

		// Not used for edit fields
		public string Title { get; init; } = "";
		public int SystemId { get; init; }
		public int? SystemFrameRateId { get; init; }
		public SubmissionStatus Status { get; init; }
		public int? GameGoalId { get; init; }

		[Display(Name = "Emulator Version")]
		public string? EmulatorVersion { get; init; }
		public string? Branch { get; init; }

		public bool CanPublish => SystemId > 0
			&& SystemFrameRateId.HasValue
			&& GameId > 0
			&& VersionId > 0
			&& GameGoalId > 0
			&& !string.IsNullOrEmpty(PublicationClass)
			&& Status == SubmissionStatus.PublicationUnderway;
	}
}
