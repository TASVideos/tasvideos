using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class HomePageHeader : ViewComponent
{
	public IViewComponentResult Invoke(IWikiPage pageData)
	{
		return View(pageData);
	}
}
