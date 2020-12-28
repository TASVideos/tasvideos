using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents
{
	public class UserGetWikiName : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
