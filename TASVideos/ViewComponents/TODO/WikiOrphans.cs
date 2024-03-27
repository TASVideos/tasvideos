using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.WikiOrphans)]
public class WikiOrphans(IWikiPages wikiPages) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var orphans = await wikiPages.Orphans();
		return View(orphans);
	}
}
