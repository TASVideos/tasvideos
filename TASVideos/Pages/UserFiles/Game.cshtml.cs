using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class GameModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public GameModel(
			ApplicationDbContext db,
			IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public GameFileModel Game { get; set; }

		[FromRoute]
		public int Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var game = await _db.Games
				.Include(g => g.System)
				.Include(g => g.UserFiles)
				.ThenInclude(u => u.Author)
				.SingleOrDefaultAsync(g => g.Id == Id);

			if (game == null)
			{
				return NotFound();
			}

			Game = new GameFileModel
			{
				GameId = game.Id,
				SystemCode = game.System.Code,
				GameName = game.DisplayName,
				Files = game.UserFiles
					.Where(uf => !uf.Hidden)
					.Select(_mapper.Map<UserFileModel>)
					.ToList()
			};

			return Page();
		}
	}
}
