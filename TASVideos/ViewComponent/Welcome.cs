using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.Welcome)]
	public class Welcome : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
