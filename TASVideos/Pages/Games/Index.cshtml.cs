using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Games.Models;

namespace TASVideos.Pages.Games
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public GameDisplayModel Game { get; set; }

		public async Task OnGet()
		{
			Game = await _db.Games
				.Where(g => g.Id == Id)
				.ProjectTo<GameDisplayModel>()
				.SingleOrDefaultAsync();
		}
	}
}
