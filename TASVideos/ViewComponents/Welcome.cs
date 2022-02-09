using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Welcome)]
public class Welcome : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
