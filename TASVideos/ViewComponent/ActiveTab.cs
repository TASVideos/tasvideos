using System;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class ActiveTab : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			// Support legacy markup: tab=b6 with a list of hardcoded translations
			// These are the names of the tabs in the legacy system so assume those names are the intent here
			// Whether we have those menus or not
			pp = pp ?? "";
			if (pp.StartsWith("tab=b"))
			{
				var val = pp.Split(new[] { "tab=b" }, StringSplitOptions.RemoveEmptyEntries);

				if (val != null && val.Length > 0 && !string.IsNullOrWhiteSpace(val[0]))
				{
					var result = int.TryParse(val[0], out int tabIndex);
					if (result)
					{
						switch (tabIndex)
						{
							case 0:
								pp = "Home";
								break;
							case 1:
								pp = "Movies";
								break;
							case 2:
								pp = "Game Resources";
								break;
							case 3:
								pp = "Articles";
								break;
							case 4:
								pp = "Emulators";
								break;
							case 5:
								pp = "Submissions";
								break;
							case 6:
								pp = "News";
								break;
							case 7:
								pp = "Forums";
								break;
							case 8:
								pp = "Chat";
								break;
							case 9:
								pp = "Staff";
								break;
							case 10:
								pp = "About";
								break;
						}
					}
				}
			}

			TempData["ActiveTab"] = pp;
			return View();
		}
	}
}