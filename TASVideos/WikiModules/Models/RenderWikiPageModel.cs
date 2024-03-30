using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class RenderWikiPageModel
{
	public string Markup { get; init; } = "";

	public IWikiPage PageData { get; init; } = null!;
}
