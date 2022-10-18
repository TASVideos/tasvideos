using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents;

public class SystemPageHeader : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
