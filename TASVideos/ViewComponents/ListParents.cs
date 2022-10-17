using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents;

public class ListParents : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public ListParents(ApplicationDbContext db)
	{
		_db = db;
	}

	public IViewComponentResult Invoke(IWikiPage pageData)
	{
		var subpages = _db.WikiPages
			.ThatAreParentsOf(pageData.PageName)
			.ToList();

		return View(subpages);
	}
}
