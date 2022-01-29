using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents;

public class RenderWikiPageModel
{
	public string Markup { get; init; } = "";

	public WikiPage PageData { get; init; } = new();
}
