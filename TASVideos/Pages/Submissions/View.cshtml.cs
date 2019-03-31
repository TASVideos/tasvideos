using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;

		public ViewModel(
			ApplicationDbContext db,
			IWikiPages wikiPages)
		{
			_db = db;
			_wikiPages = wikiPages;
		}

		[FromRoute]
		public int Id { get; set; }

		public int TopicId { get; set; }
		public int PublicationId { get; set; }

		public bool IsPublished => PublicationId > 0;

		public bool CanEdit { get; set; }

		public SubmissionDisplayModel Submission { get; set; } = new SubmissionDisplayModel();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _db.Submissions
					.Where(s => s.Id == Id)
					.Select(s => new SubmissionDisplayModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
					{
						StartType = (MovieStartType)s.MovieStartType,
						SystemDisplayName = s.System.DisplayName,
						SystemCode = s.System.Code,
						GameName = s.GameName,
						GameVersion = s.GameVersion,
						RomName = s.RomName,
						Branch = s.Branch,
						Emulator = s.EmulatorVersion,
						FrameCount = s.Frames,
						FrameRate = s.SystemFrameRate.FrameRate,
						RerecordCount = s.RerecordCount,
						CreateTimestamp = s.CreateTimeStamp,
						Submitter = s.Submitter.UserName,
						LastUpdateTimeStamp = s.WikiContent.LastUpdateTimeStamp,
						LastUpdateUser = s.WikiContent.LastUpdateUserName,
						Status = s.Status,
						EncodeEmbedLink = s.EncodeEmbedLink,
						Judge = s.Judge != null ? s.Judge.UserName : "",
						Title = s.Title,
						TierName = s.IntendedTier != null ? s.IntendedTier.Name : "",
						Publisher = s.Publisher != null ? s.Publisher.UserName : "",
						SystemId = s.SystemId,
						SystemFrameRateId = s.SystemFrameRateId,
						GameId = s.GameId,
						RomId = s.RomId,
						RejectionReasonDisplay = s.RejectionReasonId.HasValue
							? s.RejectionReason.DisplayName
							: null,
						Authors = s.SubmissionAuthors
							.Where(sa => sa.SubmissionId == Id)
							.Select(sa => sa.Author.UserName)
							.ToList()
					})
					.SingleOrDefaultAsync();

			if (Submission == null)
			{
				return NotFound();
			}

			CanEdit = !string.IsNullOrWhiteSpace(User.Identity.Name)
				&& (User.Identity.Name == Submission.Submitter
					|| Submission.Authors.Contains(User.Identity.Name));

			var submissionPageName = LinkConstants.SubmissionWikiPage + Id;
			TopicId = (await _db.ForumTopics.SingleOrDefaultAsync(t => t.PageName == submissionPageName))?.Id ?? 0;

			if (Submission.Status == SubmissionStatus.Published)
			{
				PublicationId = (await _db.Publications.SingleOrDefaultAsync(p => p.SubmissionId == Id))?.Id ?? 0;
			}

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var submissionFile = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => s.MovieFile)
				.SingleOrDefaultAsync();

			if (submissionFile == null)
			{
				return NotFound();
			}

			return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{Id}.zip");
		}

		public async Task<IActionResult> OnGetClaim()
		{
			if (!User.Has(PermissionTo.JudgeSubmissions))
			{
				return AccessDenied();
			}

			var submission = await _db.Submissions
				.Include(s => s.WikiContent)
				.SingleOrDefaultAsync(s => s.Id == Id);

			if (submission == null)
			{
				return null;
			}

			if (submission.Status != SubmissionStatus.New)
			{
				return BadRequest("Submission can not be claimed");
			}

			submission.Status = SubmissionStatus.JudgingUnderWay;
			var wikiPage = new WikiPage
			{
				PageName = submission.WikiContent.PageName,
				Markup = submission.WikiContent.Markup += $"\n----\n[user:{User.Identity.Name}]: Claiming for judging.",
				RevisionMessage = "Claiming for judging"
			};
			submission.WikiContent = wikiPage;
			submission.JudgeId = User.GetUserId();

			try
			{
				await _wikiPages.Add(wikiPage);
			}
			catch (DbUpdateConcurrencyException)
			{
				// Assume the status changed and can no longer be claimed
				return BadRequest("Submission can not be claimed");
			}

			return RedirectToPage("View", new { Id });
		}
	}
}
