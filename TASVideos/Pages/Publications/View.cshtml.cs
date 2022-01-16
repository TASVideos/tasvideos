using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IPointsService _pointsService;

		public ViewModel(
			ApplicationDbContext db,
			IMapper mapper,
			IPointsService pointsService)
		{
			_db = db;
			_mapper = mapper;
			_pointsService = pointsService;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationDisplayModel Publication { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			Publication = await _mapper
				.ProjectTo<PublicationDisplayModel>(_db.Publications.AsSingleQuery())
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (Publication == null)
			{
				return NotFound();
			}

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
