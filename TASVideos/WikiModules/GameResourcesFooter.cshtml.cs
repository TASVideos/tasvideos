using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class GameResourcesFooter : WikiViewComponent
{
	public IWikiPage WikiPage { get; set; } = null!;

	public IViewComponentResult Invoke(IWikiPage pageData)
	{
		WikiPage = pageData;
		return View();
	}
}
