using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.BannedAvatarSites)]
public class BannedAvatarSites : ViewComponent
{
	private readonly UserManager _userManager;

	public BannedAvatarSites(UserManager userManager)
	{
		_userManager = userManager;
	}

	public IViewComponentResult Invoke()
	{
		return View(_userManager.BannedAvatarSites());
	}
}
