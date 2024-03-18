﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.BannedAvatarSites)]
public class BannedAvatarSites(UserManager userManager) : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View(userManager.BannedAvatarSites());
	}
}
