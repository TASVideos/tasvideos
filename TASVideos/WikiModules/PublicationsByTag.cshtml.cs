using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationsByTag)]
public class PublicationsByTag(ITagService tags) : WikiViewComponent
{
	public IEnumerable<Tag> Tags { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Tags = await tags.GetAll();
		return View();
	}
}
