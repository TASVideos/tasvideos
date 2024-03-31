using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.BannedAvatarSites)]
public class BannedAvatarSites(UserManager userManager) : WikiViewComponent
{
	public string[] Sites { get; set; } = [];

	public IViewComponentResult Invoke()
	{
		Sites = userManager.BannedAvatarSites();
		return View();
	}
}
