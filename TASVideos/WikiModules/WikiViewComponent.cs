using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace TASVideos.WikiModules;

public abstract class WikiViewComponent : ViewComponent
{
	private string WikiViewPath => string.Format(CultureInfo.InvariantCulture, "/WikiModules/{0}.cshtml", GetType().Name);

	public new ViewViewComponentResult View()
	{
		return View(viewName: WikiViewPath, model: this);
	}
}
