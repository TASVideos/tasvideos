using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.UserGetWikiName)]
	public class UserGetWikiName : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
