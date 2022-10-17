using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.ViewComponents;

public class GameResourcesFooter : ViewComponent
{
	public IViewComponentResult Invoke(IWikiPage pageData)
	{
		return View(pageData);
	}
}
