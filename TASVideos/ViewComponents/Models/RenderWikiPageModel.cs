using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents;

public class RenderWikiPageModel
{
	public string Markup { get; init; } = "";

	public IWikiPage PageData { get; init; } = null!;
}
