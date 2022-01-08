using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
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

		public PublishModel(
			ApplicationDbContext db,
			IMapper mapper,
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			IMediaFileUploader uploader,
			ITASVideoAgent tasVideoAgent,
			UserManager userManager,
			IFileService fileService,
			IYoutubeSync youtubeSync)
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
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionPublishModel Submission { get; set; } = new ();

		public IEnumerable<SelectListItem> AvailableMoviesToObsolete { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _mapper
				.ProjectTo<SubmissionPublishModel>(
					_db.Submissions.Where(s => s.Id == Id))
				.SingleOrDefaultAsync();

			if (Submission == null)
			{
				return NotFound();
			}

			if (!Submission.CanPublish)
			{
				return AccessDenied();
			}

			await PopulateAvailableMoviesToObsolete(Submission.SystemId);
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
				await PopulateAvailableMoviesToObsolete(Submission.SystemId);
				return Page();
			}

			// TODO: I think this is producing joins, if the submission isn't properly cataloged then
			// this will throw an exception, if so, use OrDefault and return NotFound()
			// if it is doing left joins or sub-queries, then we need to null check the usages of nullable
			// tables such as game, rom, etc and throw if those are null
			var submission = await _db.Submissions
				.Include(s => s.IntendedClass)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.Game)
				.Include(s => s.Rom)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == Id);

			var publication = new Publication
			{
				PublicationClassId = submission.IntendedClass!.Id,
				SystemId = submission.System!.Id,
				SystemFrameRateId = submission.SystemFrameRate!.Id,
				GameId = submission.Game!.Id,
				RomId = submission.Rom!.Id,
				Branch = submission.Branch,
				EmulatorVersion = Submission.EmulatorVersion,
				Frames = submission.Frames,
				RerecordCount = submission.RerecordCount,
				MovieFileName = Submission.MovieFileName + "." + Submission.MovieExtension,
				AdditionalAuthors = submission.AdditionalAuthors
			};

			publication.PublicationUrls.Add(new PublicationUrl
			{
				Url = Submission.OnlineWatchingUrl,
				Type = PublicationUrlType.Streaming
			});

			publication.PublicationUrls.Add(new PublicationUrl
			{
				Url = Submission.MirrorSiteUrl,
				Type = PublicationUrlType.Mirror
			});

			publication.MovieFile = await _fileService.CopyZip(
				submission.MovieFile,
				Submission.MovieFileName + "." + Submission.MovieExtension);

			publication.Authors.AddRange(submission.SubmissionAuthors
				.Select(sa => new PublicationAuthor
				{
					Publication = publication,
					Author = sa.Author,
					Ordinal = sa.Ordinal
				}));

			publication.Submission = submission;
			_db.Publications.Add(publication);

			await _db.SaveChangesAsync(); // Need an Id for the Title
			publication.GenerateTitle();

			await _uploader.UploadScreenshot(publication.Id, Submission.Screenshot!, Submission.ScreenshotDescription);

			// Create a wiki page corresponding to this submission
			var wikiPage = new WikiPage
			{
				RevisionMessage = $"Auto-generated from Movie #{publication.Id}",
				PageName = LinkConstants.PublicationWikiPage + publication.Id,
				MinorEdit = false,
				Markup = Submission.MovieMarkup,
				AuthorId = User.GetUserId()
			};

			await _wikiPages.Add(wikiPage);
			publication.WikiContent = wikiPage;

			submission.Status = SubmissionStatus.Published;
			var history = new SubmissionStatusHistory
			{
				SubmissionId = submission.Id,
				Status = SubmissionStatus.Published
			};
			submission.History.Add(history);
			_db.SubmissionStatusHistory.Add(history);

			Publication? toObsolete = null;
			if (Submission.MovieToObsolete.HasValue)
			{
				toObsolete = await _db.Publications
					.Include(p => p.PublicationUrls)
					.Include(p => p.WikiContent)
					.Include(p => p.System)
					.Include(p => p.Game)
					.Include(p => p.Authors)
					.ThenInclude(pa => pa.Author)
					.SingleAsync(p => p.Id == Submission.MovieToObsolete);
				toObsolete.ObsoletedById = publication.Id;
			}

			await _db.SaveChangesAsync();

			var user = await _userManager.GetUserAsync(User);
			await _userManager.AssignAutoAssignableRolesByPublication(user);

			await _tasVideosAgent.PostSubmissionPublished(submission.Id, publication.Id);
			await _publisher.AnnouncePublication(publication.Title, $"{publication.Id}M", User.Name());

			if (_youtubeSync.IsYoutubeUrl(Submission.OnlineWatchingUrl))
			{
				var video = new YoutubeVideo(
					publication.Id,
					publication.CreateTimestamp,
					Submission.OnlineWatchingUrl,
					publication.Title,
					wikiPage,
					submission.System.Code,
					publication.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
					submission.Game.SearchKey,
					null);
				await _youtubeSync.SyncYouTubeVideo(video);
			}

			if (toObsolete != null)
			{
				foreach (var url in toObsolete.PublicationUrls
					.ThatAreStreaming()
					.Where(pu => _youtubeSync.IsYoutubeUrl(pu.Url)))
				{
					var obsoleteVideo = new YoutubeVideo(
						toObsolete.Id,
						toObsolete.CreateTimestamp,
						url.Url ?? "",
						toObsolete.Title,
						toObsolete.WikiContent!,
						toObsolete.System!.Code,
						toObsolete.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
						toObsolete.Game!.SearchKey,
						publication.Id);

					await _youtubeSync.SyncYouTubeVideo(obsoleteVideo);
				}
			}

			return BaseRedirect($"/{publication.Id}M");
		}

		private async Task PopulateAvailableMoviesToObsolete(int systemId)
		{
			AvailableMoviesToObsolete = await _db.Publications
				.ThatAreCurrent()
				.Where(p => p.SystemId == systemId)
				.Select(p => new SelectListItem
				{
					Value = p.Id.ToString(),
					Text = p.Title
				})
				.OrderBy(p => p.Text)
				.ToListAsync();
		}
	}
}
