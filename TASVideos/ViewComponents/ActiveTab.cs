using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.ActiveTab)]
public class ActiveTab : ViewComponent
{
	public IViewComponentResult Invoke(string? tab)
	{
		TempData["ActiveTab"] = tab;
		return View();
	}
}
