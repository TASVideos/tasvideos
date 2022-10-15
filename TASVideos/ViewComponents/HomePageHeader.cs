using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents;

public class HomePageHeader : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
