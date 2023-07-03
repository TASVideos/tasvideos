using Microsoft.AspNetCore.Mvc;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Redirect)]
public class Redirect : ViewComponent
{
	public IViewComponentResult Invoke(string page)
	{
		HttpContext context = ViewContext.HttpContext;

		string redirectValue = context.Request.Query["redirect"];

		if (redirectValue == "no")
		{
			return Content("Redirects to: " + page);
		}
		else
		{
			context.Response.Redirect("/" + page + "?redirect=no");
		}

		return Content("");
	}
}
