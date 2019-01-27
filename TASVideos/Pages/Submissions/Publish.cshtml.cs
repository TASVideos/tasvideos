using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.PublishMovies)]
	public class PublishModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;
		private readonly ApplicationDbContext _db;

		public PublishModel(
			ApplicationDbContext db,
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_submissionTasks = submissionTasks;
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
					Id = s.Id,
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
				// TODO: fix name
				ModelState.AddModelError(nameof(Submission.Screenshot), "Invalid file type. Must be .png or .jpg");
			}

			if (Submission.TorrentFile.Name != "TorrentFile")
			{
				// TODO: fix name
				ModelState.AddModelError(nameof(Submission.TorrentFile), "Invalid file type. Must be a .torrent file");
			}

			if (!ModelState.IsValid)
			{
				await PopulateAvailableMoviesToObsolete(Submission.SystemId);
				return Page();
			}

			var id = await _submissionTasks.PublishSubmission(Submission);
			return Redirect($"/{id}M");
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
