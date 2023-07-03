using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Redirect)]
public class Redirect : ViewComponent
{
	public IViewComponentResult Invoke(string page)
	{
		HttpContext context = ViewContext.HttpContext;

		context.Response.Redirect("/" + page);

		return Content("");
	}
}
