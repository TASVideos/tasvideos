using System.ComponentModel.DataAnnotations;
using TASVideos.Core;

namespace TASVideos.ViewComponents;

public class ScreenshotEntry
{
	[Display(Name = "Screenshot")]
	public string Path { get; init; } = "";

	[Sortable]
	[Display(Name = "Id")]
	public int PublicationId { get; init; }

	[Sortable]
	[Display(Name = "Title")]
	public string PublicationTitle { get; init; } = "";

	[Sortable]
	public string Description { get; init; } = "";
}

public class ScreenshotPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
{
	public bool? OnlyDescriptions { get; set; }
}
