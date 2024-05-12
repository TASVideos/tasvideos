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
			return Error("Invalid parameters.");
		}

		ShowClasses = showClassIcons;
		Publications = await db.Publications
			.ForDateRange(before.Value, after.Value)
			.Where(p => !platforms.Any() || platforms.Contains(p.SystemId))
			.Select(p => new PublicationEntry(
				p.Id,
				p.Title,
				p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				p.PublicationClass!.IconPath))
			.ToListAsync();

		return View();
	}

	public record PublicationEntry(int Id, string Title, IEnumerable<string> Authors, string? ClassIconPath);
}
