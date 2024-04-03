using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationPoints)]
public class PublicationPoints(ApplicationDbContext db, IPointsService pointsService) : WikiViewComponent
{
	public List<PointsEntry> Pubs { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var publications = await db.Publications
			.ThatAreCurrent()
			.Select(p => new PointsEntry
			{
				Id = p.Id,
				Title = p.Title
			})
			.ToListAsync();

		foreach (var pub in publications)
		{
			pub.Points = await pointsService.PlayerPointsForPublication(pub.Id);
		}

		Pubs = [.. publications.OrderByDescending(u => u.Points)];

		int counter = 0;
		foreach (var pub in Pubs)
		{
			pub.Position = ++counter;
		}

		return View();
	}

	public class PointsEntry
	{
		public int Position { get; set; }
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public double Points { get; set; }
	}
}
