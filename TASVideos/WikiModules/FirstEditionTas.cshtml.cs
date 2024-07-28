using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.FirstEditionTas)]
public class FirstEditionTas(ApplicationDbContext db) : WikiViewComponent
{
	public List<FirstEdition> Movies { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(DateTime? before, DateTime? after, bool splitByPlatform)
	{
		var beforeYear = before ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
		var afterYear = after ?? new DateTime(DateTime.UtcNow.AddYears(1).Year, 1, 1);

		List<FirstEditionGame> firstEditions;

		if (splitByPlatform)
		{
			firstEditions = await db.Publications
				.GroupBy(
					gkey => new { gkey.GameId },
					gvalue => new { gvalue.Id, gvalue.CreateTimestamp })
				.Select(g => new FirstEditionGame
				{
					GameId = g.Key.GameId,
					PublicationDate = g.Min(gg => gg.CreateTimestamp)
				})
				.Where(g => g.PublicationDate >= afterYear)
				.Where(g => g.PublicationDate < beforeYear)
				.ToListAsync();
		}
		else
		{
			firstEditions = await db.Publications
				.GroupBy(
					gkey => new { gkey.Game!.DisplayName },
					gvalue => new { gvalue.Id, gvalue.CreateTimestamp })
				.Select(g => new FirstEditionGame
				{
					GameName = g.Key.DisplayName,
					PublicationDate = g.Min(gg => gg.CreateTimestamp)
				})
				.Where(g => g.PublicationDate >= afterYear)
				.Where(g => g.PublicationDate < beforeYear)
				.ToListAsync();
		}

		var query = db.Publications
			.Where(p => p.CreateTimestamp >= afterYear)
			.Where(p => p.CreateTimestamp < beforeYear);

		if (splitByPlatform)
		{
			var firstEditionIds = firstEditions.Select(f => f.GameId).ToList();
			query = query.Where(p => firstEditionIds.Contains(p.GameId));
		}
		else
		{
			var firstEditionNames = firstEditions.Select(f => f.GameName).ToList();
			query = query.Where(p => firstEditionNames.Contains(p.Game!.DisplayName));
		}

		// TODO: first edition logic
		var pubs = await query
			.Select(p => new FirstEdition(
				p.Id,
				p.GameId,
				p.Title,
				p.PublicationClassId,
				p.PublicationClass!.IconPath,
				p.PublicationClass.Name,
				p.CreateTimestamp))
			.ToListAsync();

		// If multiple first editions in the same year, go with the first
		Movies = pubs
			.GroupBy(g => new { g.GameId })
			.Select(g => g.OrderBy(gg => gg.Date).First())
			.ToList();

		return View();
	}

	private class FirstEditionGame
	{
		public int GameId { get; init; }
		public string GameName { get; init; } = "";
		public DateTime? PublicationDate { get; init; }
	}

	public record FirstEdition(int Id, int GameId, string Title, int ClassId, string? ClassIconPath, string ClassName, DateTime Date);
}
