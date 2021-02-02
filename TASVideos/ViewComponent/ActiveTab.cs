using Microsoft.AspNetCore.Mvc;
using TASVideos.Extensions;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.ActiveTab)]
	public class ActiveTab : ViewComponent
	{
		public IViewComponentResult Invoke(string? tab)
		{
			// Support legacy markup: tab=b6 with a list of hardcoded translations
			// These are the names of the tabs in the legacy system so assume those names are the intent here
			// Whether we have those menus or not
			tab = tab switch
			{
				"b0" => "Home",
				"b1" => "Movies",
				"b2" => "Game Resources",
				"b3" => "Articles",
				"b4" => "Emulators",
				"b5" => "Submissions",
				"b6" => "News",
				"b7" => "Forums",
				"b8" => "Chat",
				"b9" => "Staff",
				"b10" => "About",
				_ => tab
			};

			TempData["ActiveTab"] = tab;
			return View();
		}
	}
}
