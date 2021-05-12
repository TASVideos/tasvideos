using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.RazorPages.Pages.Games.Models;

namespace TASVideos.RazorPages.Pages.Games
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

		public async Task OnGet()
		{
			Game = await _mapper
				.ProjectTo<GameDisplayModel>(_db.Games)
				.SingleOrDefaultAsync(g => g.Id == Id);
		}
	}
}
