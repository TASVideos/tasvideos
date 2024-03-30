using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace TASVideos.WikiModules;

public abstract class WikiViewComponent : ViewComponent
{
	public new ViewViewComponentResult View<TModel>(TModel? model)
	{
		return View(viewName: string.Format(CultureInfo.InvariantCulture, "../../../../WikiModules/{0}/{0}.cs.cshtml", GetType().Name), model: model);
	}
}
