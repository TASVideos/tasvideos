using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PlayerPointsTable)]
public class PlayerPointsTable(ApplicationDbContext db, IPointsService pointsService) : WikiViewComponent
{
	public List<PointsEntry> Authors { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? count)
	{
		var showCount = count ?? 50;

		var authors = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new PointsEntry
			{
				Id = u.Id,
				Player = u.UserName
			})
			.ToListAsync();

		foreach (var user in authors)
		{
			(user.Points, user.Rank) = await pointsService.PlayerPoints(user.Id);
		}

		Authors = authors
			.OrderByDescending(u => u.Points)
			.Take(showCount)
			.ToList();

		int counter = 0;
		foreach (var user in Authors)
		{
			user.Position = ++counter;
		}

		return View();
	}

	public class PointsEntry
	{
		public int Position { get; set; }
		public int Id { get; init; }
		public string Player { get; init; } = "";
		public double Points { get; set; }
		public string Rank { get; set; } = "";
	}
}
