using Microsoft.AspNetCore.Mvc;

namespace TASVideos.ViewComponents;

public class SystemPageFooter : ViewComponent
{
	public IViewComponentResult Invoke()
	{
		return View();
	}
}
