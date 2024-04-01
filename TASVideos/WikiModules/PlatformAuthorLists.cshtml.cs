using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PlatformAuthorList)]
public class PlatformAuthorLists(ApplicationDbContext db) : WikiViewComponent
{
	public bool ShowClasses { get; set; }
	public List<PublicationEntry> Publications { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(bool showClassIcons, DateTime? before, DateTime? after, IList<int> platforms)
	{
		if (!before.HasValue || !after.HasValue)
		{
			return new ContentViewComponentResult("Invalid parameters.");
		}

		ShowClasses = showClassIcons;
		Publications = await db.Publications
			.ForDateRange(before.Value, after.Value)
			.Where(p => !platforms.Any() || platforms.Contains(p.SystemId))
			.Select(p => new PublicationEntry
			{
				Id = p.Id,
				Title = p.Title,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				ClassIconPath = p.PublicationClass!.IconPath
			})
			.ToListAsync();

		return View();
	}

	public class PublicationEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public IEnumerable<string> Authors { get; init; } = [];
		public string? ClassIconPath { get; init; } = "";
	}
}
