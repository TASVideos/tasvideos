using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class PlayerPointsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPointsService _pointsService;

		public PlayerPointsModel(
			ApplicationDbContext db,
			IPointsService pointsService)
		{
			_db = db;
			_pointsService = pointsService;
		}

		public IEnumerable<PlayerEntry> Players { get; set; } = new List<PlayerEntry>();

		public async Task OnGet()
		{
			Players = await _db.Users
				.Where(u => u.Publications.Any())
				.Select(u => new PlayerEntry
				{
					Id = u.Id,
					UserName = u.UserName
				})
				.ToListAsync();

			foreach (var user in Players)
			{
				user.Points = await _pointsService.PlayerPoints(user.Id);
			}
		}

		public class PlayerEntry
		{
			public int Id { get; set; }
			public string UserName { get; set; }
			public double Points { get; set; }
		}
	}
}
