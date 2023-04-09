using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents;

public class GameResourcesHeader : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
