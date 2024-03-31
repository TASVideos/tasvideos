using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiOrphans)]
public class WikiOrphans(IWikiPages wikiPages) : WikiViewComponent
{
	public IReadOnlyCollection<WikiOrphan> Orphans { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Orphans = await wikiPages.Orphans();
		return View();
	}
}
