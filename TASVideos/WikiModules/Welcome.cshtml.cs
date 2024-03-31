using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Welcome)]
public class Welcome : WikiViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
