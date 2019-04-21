using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.PublishMovies)]
	public class PublishModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IWikiPages _wikiPages;
		private readonly IMediaFileUploader _uploader;

		public PublishModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			IMediaFileUploader uploader)
		{
			_db = db;
			_publisher = publisher;
			_wikiPages = wikiPages;
			_uploader = uploader;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionPublishModel Submission { get; set; } = new SubmissionPublishModel();

		public IEnumerable<SelectListItem> AvailableMoviesToObsolete { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Submission = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new SubmissionPublishModel
				{
					Title = s.Title,
					Markup = s.WikiContent.Markup,
					SystemCode = s.System.Code,
					SystemId = s.SystemId ?? 0,
					SystemFrameRateId = s.SystemFrameRateId,
					SystemRegion = s.SystemFrameRate.RegionCode + " " + s.SystemFrameRate.FrameRate,
					Game = s.Game.GoodName,
					GameId = s.GameId ?? 0,
					Rom = s.Rom.Name,
					RomId = s.RomId ?? 0,
					Tier = s.IntendedTier != null ? s.IntendedTier.Name : "",
					Branch = s.Branch,
					EmulatorVersion = s.EmulatorVersion,
					MovieExtension = s.MovieExtension,
					Status = s.Status
				})
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
			if (Submission.Screenshot.ContentType != "image/png"
				&& Submission.Screenshot.ContentType != "image/jpeg")
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.Screenshot)}", "Invalid file type. Must be .png or .jpg");
			}

			if (!Submission.TorrentFile.FileName.EndsWith(".torrent"))
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.TorrentFile)}", "Invalid file type. Must be a .torrent file");
			}

			if (!ModelState.IsValid)
			{
				await PopulateAvailableMoviesToObsolete(Submission.SystemId);
				return Page();
			}

			var submission = await _db.Submissions
				.Include(s => s.IntendedTier)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.Game)
				.Include(s => s.Rom)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == Id);

			var publication = new Publication
			{
				TierId = submission.IntendedTier.Id,
				SystemId = submission.System.Id,
				SystemFrameRateId = submission.SystemFrameRate.Id,
				GameId = submission.Game.Id,
				RomId = submission.Rom.Id,
				Branch = submission.Branch,
				EmulatorVersion = Submission.EmulatorVersion,
				OnlineWatchingUrl = Submission.OnlineWatchingUrl,
				MirrorSiteUrl = Submission.MirrorSiteUrl,
				Frames = submission.Frames,
				RerecordCount = submission.RerecordCount,
				MovieFileName = Submission.MovieFileName + "." + Submission.MovieExtension
			};

			// Unzip the submission file, and re-zip it while renaming the contained file
			using (var publicationFileStream = new MemoryStream())
			{
				using (var publicationZipArchive = new ZipArchive(publicationFileStream, ZipArchiveMode.Create))
				using (var submissionFileStream = new MemoryStream(submission.MovieFile))
				using (var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read))
				{
					var publicationZipEntry = publicationZipArchive.CreateEntry(Submission.MovieFileName + "." + Submission.MovieExtension);
					var submissionZipEntry = submissionZipArchive.Entries.Single();

					using (var publicationZipEntryStream = publicationZipEntry.Open())
					using (var submissionZipEntryStream = submissionZipEntry.Open())
					{
						await submissionZipEntryStream.CopyToAsync(publicationZipEntryStream);
					}
				}

				publication.MovieFile = publicationFileStream.ToArray();
			}

			var publicationAuthors = submission.SubmissionAuthors
				.Select(sa => new PublicationAuthor
				{
					Publication = publication,
					Author = sa.Author
				});

			foreach (var author in publicationAuthors)
			{
				publication.Authors.Add(author);
			}

			publication.Submission = submission;
			_db.Publications.Add(publication);

			await _db.SaveChangesAsync(); // Need an Id for the Title
			publication.GenerateTitle();

			// TODO: screenshot description
			await _uploader.UploadScreenshot(publication.Id, Submission.Screenshot, null);
			await _uploader.UploadTorrent(publication.Id, Submission.TorrentFile);

			// Create a wiki page corresponding to this submission
			var wikiPage = new WikiPage
			{
				RevisionMessage = $"Auto-generated from Movie #{publication.Id}",
				PageName = LinkConstants.PublicationWikiPage + publication.Id,
				MinorEdit = false,
				Markup = Submission.MovieMarkup
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

			if (Submission.MovieToObsolete.HasValue)
			{
				var toObsolete = await _db.Publications.SingleAsync(p => p.Id == Submission.MovieToObsolete);
				toObsolete.ObsoletedById = publication.Id;
			}

			await _db.SaveChangesAsync();

			// Create post
			var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.PageName == LinkConstants.SubmissionWikiPage + submission.Id);
			if (topic != null)
			{
				_db.ForumPosts.Add(new ForumPost
				{
					TopicId = topic.Id,
					CreateUserName = SiteGlobalConstants.TASVideoAgent,
					LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
					PosterId = SiteGlobalConstants.TASVideoAgentId,
					EnableBbCode = false,
					EnableHtml = true,
					Subject = SiteGlobalConstants.NewPublicationPostSubject,
					Text = SiteGlobalConstants.NewPublicationPost.Replace("{PublicationId}", publication.Id.ToString())
				});
				await _db.SaveChangesAsync();
			}

			_publisher.AnnouncePublication(publication.Title, $"{BaseUrl}/{publication.Id}M");

			return Redirect($"/{publication.Id}M");
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
				.ToListAsync();
		}
	}
}
