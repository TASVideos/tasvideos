using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;

		public ViewModel(
			ApplicationDbContext db,
			IWikiPages wikiPages,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_wikiPages = wikiPages;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		public int TopicId { get; set; }
		public int PublicationId { get; set; }

		public bool IsPublished => PublicationId > 0;

		public bool CanEdit { get; set; }

		public SubmissionDisplayModel Submission { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new SubmissionDisplayModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
				{
					StartType = (MovieStartType?)s.MovieStartType,
					SystemDisplayName = s.System!.DisplayName,
					SystemCode = s.System.Code,
					GameName = s.GameName,
					GameVersion = s.GameVersion,
					RomName = s.RomName,
					Branch = s.Branch,
					Emulator = s.EmulatorVersion,
					FrameCount = s.Frames,
					FrameRate = s.SystemFrameRate!.FrameRate,
					RerecordCount = s.RerecordCount,
					Submitted = s.CreateTimestamp,
					Submitter = s.Submitter!.UserName,
					LastUpdateTimestamp = s.WikiContent!.LastUpdateTimestamp,
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
						? s.RejectionReason!.DisplayName
						: null,
					Authors = s.SubmissionAuthors
						.Where(sa => sa.SubmissionId == Id)
						.Select(sa => sa.Author!.UserName)
						.ToList(),
					AdditionalAuthors = s.AdditionalAuthors
				})
				.SingleOrDefaultAsync();

			if (Submission == null)
			{
				return NotFound();
			}

			CanEdit = !string.IsNullOrWhiteSpace(User.Name())
				&& (User.Name() == Submission.Submitter
					|| Submission.Authors.Contains(User.Name()));
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

		public async Task<IActionResult> OnGetClaimForJudging()
		{
			if (!User.Has(PermissionTo.JudgeSubmissions))
			{
				return AccessDenied();
			}

			return await Claim(SubmissionStatus.New, SubmissionStatus.JudgingUnderWay, "judging", "Claiming for judging.", true);
		}

		public async Task<IActionResult> OnGetClaimForPublishing()
		{
			if (!User.Has(PermissionTo.PublishMovies))
			{
				return AccessDenied();
			}

			return await Claim(SubmissionStatus.Accepted, SubmissionStatus.PublicationUnderway, "publication", "Processing...", false);
		}

		private async Task<IActionResult> Claim(SubmissionStatus requiredStatus, SubmissionStatus newStatus, string action, string message, bool isJudge)
		{
			var submission = await _db.Submissions
				.Include(s => s.WikiContent)
				.SingleOrDefaultAsync(s => s.Id == Id);

			if (submission == null)
			{
				return NotFound();
			}

			if (submission.Status != requiredStatus)
			{
				return BadRequest("Submission can not be claimed");
			}

			submission.Status = newStatus;
			var wikiPage = new WikiPage
			{
				PageName = submission.WikiContent!.PageName,
				Markup = submission.WikiContent.Markup += $"\n----\n[user:{User.Name()}]: {message}",
				RevisionMessage = $"Claimed for {action}"
			};
			await _wikiPages.Add(wikiPage);
			submission.WikiContentId = wikiPage.Id;

			if (isJudge)
			{
				submission.JudgeId = User.GetUserId();
			}
			else
			{
				submission.PublisherId = User.GetUserId();
			}

			var result = await ConcurrentSave(_db, "", "Unable to claim");
			if (result)
			{
				_publisher.SendSubmissionEdit(
					$"Submission {submission.Title} set to {newStatus.EnumDisplayName()} by {User.Name()}",
					$"{Id}S",
					User.Name());
			}

			return RedirectToPage("View", new { Id });
		}
	}
}
