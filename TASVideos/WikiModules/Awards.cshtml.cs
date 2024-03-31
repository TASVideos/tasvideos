using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Awards)]
public class Awards(IAwards awards) : WikiViewComponent
{
	public ICollection<AwardAssignment> Assignments { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int year)
	{
		Assignments = await awards.ForYear(year);
		return View();
	}
}
