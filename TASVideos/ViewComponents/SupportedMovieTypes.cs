using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.SupportedMovieTypes)]
public class SupportedMovieTypes : ViewComponent
{
	private readonly IMovieFormatDeprecator _deprecator;

	public SupportedMovieTypes(IMovieFormatDeprecator deprecator)
	{
		_deprecator = deprecator;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		return View(await _deprecator.GetAll());
	}
}
