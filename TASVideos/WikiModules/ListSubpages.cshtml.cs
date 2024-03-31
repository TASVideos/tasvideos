using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class ListSubPages(ApplicationDbContext db) : WikiViewComponent
{
	public List<string> Pages { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(IWikiPage pageData, bool show)
	{
		if (string.IsNullOrWhiteSpace(pageData.PageName))
		{
			return Content("");
		}

		Pages = await db.WikiPages
			.ThatAreSubpagesOf(pageData.PageName)
			.Select(w => w.PageName)
			.ToListAsync();

		ViewData["Parent"] = pageData.PageName;

		if (show)
		{
			ViewData["show"] = true;
		}

		return View();
	}
}
