using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationsByTag)]
public class PublicationsByTag(ITagService tags) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var tags1 = await tags.GetAll();
		return View(tags1);
	}
}
