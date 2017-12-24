using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class ActiveTab : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			// TODO
			return View();
		}
	}
}