using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class Welcome : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			return View();
		}
	}
}
