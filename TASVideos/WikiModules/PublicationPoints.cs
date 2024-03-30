using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationPoints)]
public class PublicationPoints(ApplicationDbContext db, IPointsService pointsService) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var publications = await db.Publications
			.ThatAreCurrent()
			.Select(p => new PublicationPointsModel
			{
				Id = p.Id,
				Title = p.Title,
			})
			.ToListAsync();

		foreach (var pub in publications)
		{
			pub.Points = await pointsService.PlayerPointsForPublication(pub.Id);
		}

		var sortedPublications = publications
			.OrderByDescending(u => u.Points)
			.ToList();

		int counter = 0;
		foreach (var pub in sortedPublications)
		{
			pub.Position = ++counter;
		}

		return View(sortedPublications);
	}

	public class PublicationPointsModel
	{
		[Display(Name = "Pos")]
		public int Position { get; set; } = 0;

		[Display(Name = "Movie Id")]
		public int Id { get; init; } = 0;

		[Display(Name = "Movie")]
		public string Title { get; init; } = "";

		[Display(Name = "Points")]
		public double Points { get; set; } = 0.0;
	}
}
