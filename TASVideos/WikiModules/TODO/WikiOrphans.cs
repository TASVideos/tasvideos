using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules.TODO;

[WikiModule(ModuleNames.WikiOrphans)]
public class WikiOrphans(IWikiPages wikiPages) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var orphans = await wikiPages.Orphans();
		return View(orphans);
	}
}
