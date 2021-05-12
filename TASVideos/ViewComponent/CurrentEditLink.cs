using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.CurrentEditLink)]
	public class CurrentEditLink : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData)
		{
			return View(pageData);
		}
	}
}
