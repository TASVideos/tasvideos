using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.SupportedMovieTypes)]
public class SupportedMovieTypes(IMovieFormatDeprecator deprecator) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		return View(await deprecator.GetAll());
	}
}
