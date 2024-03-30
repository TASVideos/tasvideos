using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserGetWikiName)]
public class UserGetWikiName : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
