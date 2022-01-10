using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Games.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Pages.Games
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public IndexModel(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[FromRoute]
		public int Id { get; set; }

		public GameDisplayModel Game { get; set; } = new ();

		public IEnumerable<MiniMovieModel> Movies { get; set; } = new List<MiniMovieModel>();

		public async Task<IActionResult> OnGet()
		{
			Game = await _mapper
				.ProjectTo<GameDisplayModel>(_db.Games)
				.SingleOrDefaultAsync(g => g.Id == Id);

			if (Game == null)
			{
				return NotFound();
			}

			Movies = await _db.Publications
				.Where(p => p.GameId == Id && p.ObsoletedById == null)
				.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
				.ThenBy(p => p.Frames)
				.Select(p => new MiniMovieModel
				{
					Id = p.Id,
					Title = p.Title,
					Branch = p.Branch ?? "",
					Screenshot = p.Files
						.Where(f => f.Type == FileType.Screenshot)
						.Select(f => new MiniMovieModel.ScreenshotFile
						{
							Path = f.Path,
							Description = f.Description
						})
						.First(),
					OnlineWatchingUrl = p.PublicationUrls.First(u => u.Type == PublicationUrlType.Streaming).Url
				})
				.ToListAsync();

			return Page();
		}
	}
}
