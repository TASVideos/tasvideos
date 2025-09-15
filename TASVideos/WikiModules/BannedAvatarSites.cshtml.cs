using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.BannedAvatarSites)]
public class BannedAvatarSites(IUserManager userManager) : WikiViewComponent
{
	public string[] Sites { get; set; } = [];

	public IViewComponentResult Invoke()
	{
		Sites = userManager.GetBannedAvatarSites();
		return View();
	}
}
