using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ViewModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public SubmissionDisplayModel Submission { get; set; } = new SubmissionDisplayModel();

		public async Task<IActionResult> OnGet()
		{
			Submission = await _db.Submissions
					.Where(s => s.Id == Id)
					.Select(s => new SubmissionDisplayModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
					{
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

			Submission.CanEdit = !string.IsNullOrWhiteSpace(User.Identity.Name)
				&& (User.Identity.Name == Submission.Submitter
					|| Submission.Authors.Contains(User.Identity.Name));

			var submissionPageName = LinkConstants.SubmissionWikiPage + Id;
			Submission.TopicId = _db.ForumTopics.SingleOrDefault(t => t.PageName == submissionPageName)?.Id ?? 0;

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
	}
}
