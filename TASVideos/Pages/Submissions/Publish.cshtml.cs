using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.PublishMovies)]
public class PublishModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IWikiPages _wikiPages;
	private readonly IMediaFileUploader _uploader;
	private readonly ITASVideoAgent _tasVideosAgent;
	private readonly UserManager _userManager;
	private readonly IFileService _fileService;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IQueueService _queueService;

	public PublishModel(
		ApplicationDbContext db,
		IMapper mapper,
		ExternalMediaPublisher publisher,
		IWikiPages wikiPages,
		IMediaFileUploader uploader,
		ITASVideoAgent tasVideoAgent,
		UserManager userManager,
		IFileService fileService,
		IYoutubeSync youtubeSync,
		IQueueService queueService)
	{
		_db = db;
		_mapper = mapper;
		_publisher = publisher;
		_wikiPages = wikiPages;
		_uploader = uploader;
		_tasVideosAgent = tasVideoAgent;
		_userManager = userManager;
		_fileService = fileService;
		_youtubeSync = youtubeSync;
		_queueService = queueService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SubmissionPublishModel Submission { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableMoviesToObsolete { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();
	public IEnumerable<SelectListItem> AvailableFlags { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var submission = await _mapper
			.ProjectTo<SubmissionPublishModel>(
				_db.Submissions.Where(s => s.Id == Id))
			.SingleOrDefaultAsync();

		if (submission == null)
		{
			return NotFound();
		}

		if (!submission.CanPublish)
		{
			return AccessDenied();
		}

		Submission = submission;

		await PopulateDropdowns(Submission.SystemId);
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
			await PopulateDropdowns(Submission.SystemId);
			return Page();
		}

		var submission = await _db.Submissions
			.Include(s => s.IntendedClass)
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.Rom)
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.SingleOrDefaultAsync(s => s.Id == Id);

		if (submission is null || !submission.CanPublish())
		{
			return NotFound();
		}

		var movieFileName = Submission.MovieFileName + "." + Submission.MovieExtension;
		var publication = new Publication
		{
			PublicationClassId = submission.IntendedClass!.Id,
			SystemId = submission.System!.Id,
			SystemFrameRateId = submission.SystemFrameRate!.Id,
			GameId = submission.Game!.Id,
			RomId = submission.Rom!.Id,
			Branch = submission.Branch,
			EmulatorVersion = submission.EmulatorVersion,
			Frames = submission.Frames,
			RerecordCount = submission.RerecordCount,
			MovieFileName = movieFileName,
			AdditionalAuthors = submission.AdditionalAuthors,
			Submission = submission,
			MovieFile = await _fileService.CopyZip(submission.MovieFile, movieFileName)
		};

		publication.PublicationUrls.AddStreaming(Submission.OnlineWatchingUrl, Submission.OnlineWatchUrlName);
		publication.PublicationUrls.AddMirror(Submission.MirrorSiteUrl);
		publication.Authors.CopyFromSubmission(submission.SubmissionAuthors);
		publication.PublicationFlags.AddFlags(Submission.SelectedFlags);
		publication.PublicationTags.AddTags(Submission.SelectedTags);

		_db.Publications.Add(publication);

		await _db.SaveChangesAsync(); // Need an Id for the Title
		publication.GenerateTitle();

		await _uploader.UploadScreenshot(publication.Id, Submission.Screenshot!, Submission.ScreenshotDescription);

		// Create a wiki page corresponding to this publication
		var wikiPage = GenerateWiki(publication.Id, Submission.MovieMarkup, User.GetUserId());
		await _wikiPages.Add(wikiPage);
		publication.WikiContent = wikiPage;

		submission.Status = SubmissionStatus.Published;
		_db.SubmissionStatusHistory.Add(Id, SubmissionStatus.Published);

		if (Submission.MovieToObsolete.HasValue)
		{
			await _queueService.ObsoleteWith(Submission.MovieToObsolete.Value, publication.Id);
		}

		await _userManager.AssignAutoAssignableRolesByPublication(publication.Authors.Select(pa => pa.UserId));
		await _tasVideosAgent.PostSubmissionPublished(Id, publication.Id);
		await _publisher.AnnouncePublication(publication.Title, $"{publication.Id}M");

		if (_youtubeSync.IsYoutubeUrl(Submission.OnlineWatchingUrl))
		{
			var video = new YoutubeVideo(
				publication.Id,
				publication.CreateTimestamp,
				Submission.OnlineWatchingUrl,
				Submission.OnlineWatchUrlName,
				publication.Title,
				wikiPage,
				submission.System.Code,
				publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				submission.Game.SearchKey,
				null);
			await _youtubeSync.SyncYouTubeVideo(video);
		}

		return BaseRedirect($"/{publication.Id}M");
	}

	private static WikiPage GenerateWiki(int publicationId, string markup, int userId)
	{
		return new WikiPage
		{
			RevisionMessage = $"Auto-generated from Movie #{publicationId}",
			PageName = LinkConstants.PublicationWikiPage + publicationId,
			MinorEdit = false,
			Markup = markup,
			AuthorId = userId
		};
	}

	private async Task PopulateDropdowns(int systemId)
	{
		AvailableFlags = await _db.Flags
			.ToDropDown(User.Permissions())
			.ToListAsync();
		AvailableTags = await _db.Tags
			.ToDropdown()
			.ToListAsync();
		AvailableMoviesToObsolete = await _db.Publications
			.ThatAreCurrent()
			.Where(p => p.SystemId == systemId)
			.ToDropdown()
			.OrderBy(p => p.Text)
			.ToListAsync();
	}
}
