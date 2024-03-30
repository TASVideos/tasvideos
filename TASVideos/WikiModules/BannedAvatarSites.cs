using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.BannedAvatarSites)]
public class BannedAvatarSites(UserManager userManager) : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View(userManager.BannedAvatarSites());
	}
}
