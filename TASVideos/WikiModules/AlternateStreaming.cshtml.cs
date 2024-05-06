using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.AlternateStreaming)]
public class AlternateStreaming(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Publications { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Publications = await db.Publications
			.Where(p => p.PublicationUrls.Count(u => u.Type == PublicationUrlType.Streaming) > 1)
			.Select(p => new Entry(p.Id, p.Title, p.PublicationUrls.Where(u => u.Type == PublicationUrlType.Streaming).ToList()))
			.ToListAsync();

		return View();
	}

	public record Entry(int Id, string Title, List<PublicationUrl> Urls);
}
