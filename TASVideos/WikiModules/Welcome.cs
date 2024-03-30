using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Welcome)]
public class Welcome : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
