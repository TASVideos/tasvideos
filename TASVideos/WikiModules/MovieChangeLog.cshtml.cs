using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MovieChangeLog)]
public class MovieChangeLog(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<HistoryEntry> Logs { get; set; } = new([], new());

	public async Task<IViewComponentResult> InvokeAsync(string pubClass)
	{
		var query = db.Publications.AsQueryable();

		var publicationClass = await db.PublicationClasses.FirstOrDefaultAsync(c => c.Name == pubClass);
		if (publicationClass is not null)
		{
			query = query.Where(p => p.PublicationClassId == publicationClass.Id);
		}

		Logs = await query
			.OrderByDescending(p => p.CreateTimestamp)
			.Select(p => new HistoryEntry
			{
				Date = p.CreateTimestamp.Date,
				Pubs = new()
				{
					new()
					{
						Id = p.Id,
						Name = p.Title,
						IsNewGame = p.Game != null && p.Game.Publications.OrderBy(gp => gp.CreateTimestamp).FirstOrDefault() == p,
						IsNewBranch = p.ObsoletedMovies.Count == 0,
						ClassIconPath = p.PublicationClass!.IconPath
					}
				}
			})
			.PageOf(GetPaging());

		return View();
	}

	public class HistoryEntry
	{
		public DateTime Date { get; init; }
		public List<PublicationEntry> Pubs { get; init; } = [];

		public class PublicationEntry
		{
			public int Id { get; init; }
			public string Name { get; init; } = "";
			public bool IsNewGame { get; init; }
			public bool IsNewBranch { get; init; }
			public string? ClassIconPath { get; init; }
		}
	}
}
