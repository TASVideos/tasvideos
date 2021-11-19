using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.DailyMotion)]
	public class DailyMotion : ViewComponent
	{
		public IViewComponentResult Invoke(string v, int w, int h)
		{
			return new ContentViewComponentResult("Warning: Embeds of dailymotion are no longer supported.");
		}
	}
}
