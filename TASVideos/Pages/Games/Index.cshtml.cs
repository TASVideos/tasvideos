using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Games.Models;
using TASVideos.Pages.Publications.Models;

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

		public IEnumerable<PublicationDisplayModel> Movies { get; set; } = new List<PublicationDisplayModel>();

		public async Task<IActionResult> OnGet()
		{
			Game = await _mapper
				.ProjectTo<GameDisplayModel>(_db.Games)
				.SingleOrDefaultAsync(g => g.Id == Id);

			if (Game == null)
			{
				return NotFound();
			}

			Movies = await _mapper.ProjectTo<PublicationDisplayModel>(
				_db.Publications
					.Where(p => p.GameId == Id && p.ObsoletedById == null))
					.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
				.ToListAsync();

			return Page();
		}
	}
}
