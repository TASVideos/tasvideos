namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class PlayerPointsModel(ApplicationDbContext db, IPointsService pointsService) : BasePageModel
{
	public List<PlayerEntry> Players { get; set; } = [];

	public async Task OnGet()
	{
		Players = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new PlayerEntry(u.Id, u.UserName))
			.ToListAsync();

		foreach (var user in Players)
		{
			(user.Points, user.Rank) = await pointsService.PlayerPoints(user.Id);
		}
	}

	public record PlayerEntry(int Id, string UserName)
	{
		public double Points { get; set; }
		public string Rank { get; set; } = "";
	}
}
