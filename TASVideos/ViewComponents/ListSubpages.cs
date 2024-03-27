using TASVideos.Core.Services.Wiki;

namespace TASVideos.ViewComponents;

public class ListSubPages(ApplicationDbContext db) : ViewComponent
{
	public IViewComponentResult Invoke(IWikiPage pageData, bool show)
	{
		if (string.IsNullOrWhiteSpace(pageData.PageName))
		{
			return Content("");
		}

		var subpages = db.WikiPages
			.ThatAreSubpagesOf(pageData.PageName)
			.Select(w => w.PageName)
			.ToList();

		ViewData["Parent"] = pageData.PageName;

		if (show)
		{
			ViewData["show"] = true;
		}

		return View(subpages);
	}
}
