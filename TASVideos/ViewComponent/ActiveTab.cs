using System;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents
{
	public class ActiveTab : ViewComponent
	{
		public IViewComponentResult Invoke(string pp)
		{
			// Support legacy markup: tab=b6 with a list of hardcoded translations
			// These are the names of the tabs in the legacy system so assume those names are the intent here
			// Whether we have those menus or not
			if (pp.StartsWith("tab=b"))
			{
				var val = pp.Split(new[] { "tab=b" }, StringSplitOptions.RemoveEmptyEntries);

				if (val.Length > 0 && !string.IsNullOrWhiteSpace(val[0]))
				{
					var result = int.TryParse(val[0], out int tabIndex);
					if (result)
					{
						pp = tabIndex switch
						{
							0 => "Home",
							1 => "Movies",
							2 => "Game Resources",
							3 => "Articles",
							4 => "Emulators",
							5 => "Submissions",
							6 => "News",
							7 => "Forums",
							8 => "Chat",
							9 => "Staff",
							10 => "About",
							_ => pp
						};
					}
				}
			}

			TempData["ActiveTab"] = pp;
			return View();
		}
	}
}