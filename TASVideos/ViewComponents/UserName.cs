using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents;

public class UserName : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
