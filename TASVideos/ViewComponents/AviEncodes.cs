using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.AviEncodes)]
	public class AviEncodes : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return new ContentViewComponentResult($"{WikiModules.AviEncodes} wiki module no longer supported");
		}
	}
}
