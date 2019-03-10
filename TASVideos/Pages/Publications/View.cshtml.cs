using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPointsCalculator _pointsCalculator;

		public ViewModel(
			ApplicationDbContext db,
			IPointsCalculator pointsCalculator)
		{
			_db = db;
			_pointsCalculator = pointsCalculator;
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

			Publication.OverallRating = (await _pointsCalculator.PublicationRating(Id))
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
	}
}
