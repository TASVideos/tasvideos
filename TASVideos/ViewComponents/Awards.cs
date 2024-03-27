using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Awards)]
public class Awards(IAwards awards) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int year)
	{
		var model = await awards.ForYear(year);
		return View(model);
	}
}
