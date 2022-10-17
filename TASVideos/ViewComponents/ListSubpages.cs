using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents;

public class ListSubPages : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public ListSubPages(ApplicationDbContext db)
	{
		_db = db;
	}

	public IViewComponentResult Invoke(IWikiPage pageData, bool show)
	{
		if (string.IsNullOrWhiteSpace(pageData.PageName))
		{
			return Content("");
		}

		var subpages = _db.WikiPages
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
