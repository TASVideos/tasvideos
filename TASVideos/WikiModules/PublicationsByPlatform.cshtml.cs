using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationsByPlatform)]
public class PublicationsByPlatform(ApplicationDbContext db) : WikiViewComponent
{
	public List<PlatformPublications> PlatformPublicationList { get; set; } = [];
	public List<string> AllGroupings { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(IList<string> flags, IList<string> order)
	{
		PlatformPublicationList = await db.Publications
			.ThatAreCurrent()
			.GroupBy(p => p.System)
			.Select(g => new PlatformPublications
			{
				Platform = g.Key!.DisplayName,
				PlatformCode = g.Key.Code,
				Groupings = g
					.GroupBy(p => p.PublicationClass)
					.Select(gg => new PlatformPublications.Grouping
					{
						IsClass = true,
						Name = gg.Key!.Name,
						Link = gg.Key.Link,
						PublicationCount = gg.Count()
					})
					.ToList()
			})
			.ToListAsync();

		var flagPublications = await db.PublicationFlags
			.Where(pf => flags.Contains(pf.Flag!.Token))
			.Where(pf => pf.Publication!.ObsoletedById == null)
			.GroupBy(pf => pf.Publication!.System)
			.Select(g => new PlatformPublications
			{
				Platform = g.Key!.DisplayName,
				PlatformCode = g.Key.Code,
				Groupings = g
					.GroupBy(p => p.Flag)
					.Select(gg => new PlatformPublications.Grouping
					{
						IsClass = false,
						Name = gg.Key!.Name,
						Link = gg.Key.Token ?? "",
						PublicationCount = gg.Count()
					})
					.ToList()
			})
			.ToListAsync();

		PlatformPublicationList = PlatformPublicationList // merge platforms and flags
			.Concat(flagPublications)
			.GroupBy(p => p.Platform)
			.OrderBy(g => g.Key, StringComparer.InvariantCultureIgnoreCase)
			.Select(g => new PlatformPublications
			{
				Platform = g.Key,
				PlatformCode = g.First().PlatformCode,
				Groupings = g
					.SelectMany(p => p.Groupings)
					.ToList()
			})
			.ToList();

		AllGroupings = PlatformPublicationList
			.SelectMany(p => p.Groupings.Select(c => c.Link))
			.Distinct()
			.OrderBy(g => g, StringComparer.InvariantCultureIgnoreCase)
			.ToList();

		var swapPosition = 0;
		foreach (var o in order)
		{
			var index = AllGroupings.FindIndex(g => g.Equals(o, StringComparison.InvariantCultureIgnoreCase));
			if (index != -1 && index >= swapPosition)
			{
				(AllGroupings[swapPosition], AllGroupings[index]) = (AllGroupings[index], AllGroupings[swapPosition]);
				swapPosition++;
			}
		}

		return View();
	}

	public class PlatformPublications
	{
		public string Platform { get; init; } = "";
		public string PlatformCode { get; init; } = "";
		public List<Grouping> Groupings { get; init; } = [];

		public class Grouping
		{
			public bool IsClass { get; init; }
			public string Name { get; init; } = "";
			public string Link { get; init; } = "";
			public int PublicationCount { get; init; }
		}
	}
}
