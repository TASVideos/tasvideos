using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class PlayerPointsModel(
	ApplicationDbContext db,
	IPointsService pointsService) : BasePageModel
{
	public IEnumerable<PlayerEntry> Players { get; set; } = new List<PlayerEntry>();

	public async Task OnGet()
	{
		Players = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new PlayerEntry
			{
				Id = u.Id,
				UserName = u.UserName
			})
			.ToListAsync();

		foreach (var user in Players)
		{
			(user.Points, user.Rank) = await pointsService.PlayerPoints(user.Id);
		}
	}

	public class PlayerEntry
	{
		public int Id { get; set; }
		public string UserName { get; set; } = "";
		public double Points { get; set; }
		public string Rank { get; set; } = "";
	}
}
