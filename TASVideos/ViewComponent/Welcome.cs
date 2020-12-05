using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents
{
	public class Welcome : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
