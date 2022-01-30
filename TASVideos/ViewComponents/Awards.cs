using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Awards)]
public class Awards : ViewComponent
{
	private readonly IAwards _awards;

	public Awards(IAwards awards)
	{
		_awards = awards;
	}

	public async Task<IViewComponentResult> InvokeAsync(int year)
	{
		var model = await _awards.ForYear(year);
		return View(model);
	}
}
