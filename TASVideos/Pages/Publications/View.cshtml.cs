using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPointsService _pointsService;

		public ViewModel(
			ApplicationDbContext db,
			IPointsService pointsService)
		{
			_db = db;
			_pointsService = pointsService;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationDisplayModel Publication { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
				.ProjectTo<PublicationDisplayModel>()
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (Publication == null)
			{
				return NotFound();
			}

			var pageName = LinkConstants.SubmissionWikiPage + Publication.SubmissionId;
			Publication.TopicId = (await _db.ForumTopics
					.SingleOrDefaultAsync(t => t.PageName == pageName))
					?.Id ?? 0;

			Publication.OverallRating = (await _pointsService.PublicationRating(Id))
				.Overall;

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var pub = await _db.Publications
				.Where(s => s.Id == Id)
				.Select(s => new { s.MovieFile, s.MovieFileName })
				.SingleOrDefaultAsync();

			if (pub == null)
			{
				return NotFound();
			}

			return File(pub.MovieFile, MediaTypeNames.Application.Octet, $"{pub.MovieFileName}.zip");
		}

		public async Task<IActionResult> OnGetDownloadAdditional(int fileId)
		{
			var file = await _db.PublicationFiles
				.Where(pf => pf.Id == fileId)
				.Select(pf => new { pf.FileData, pf.Path })
				.SingleOrDefaultAsync();

			if (file == null)
			{
				return NotFound();
			}

			return File(file.FileData, MediaTypeNames.Application.Octet, $"{file.Path}.zip");
		}
	}
}
