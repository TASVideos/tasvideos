using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class WikiModel(ApplicationDbContext db) : BasePageModel
{
	public List<RssWiki> WikiEdits { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		WikiEdits = await db.WikiPages
			.ByMostRecent()
			.Select(wp => new RssWiki(
				wp.RevisionMessage ?? "",
				wp.PageName,
				wp.LastUpdateTimestamp,
				wp.Revision == 1,
				wp.Author!.UserName))
			.Take(10)
			.ToListAsync();
		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}

	public record RssWiki(string RevisionMessage, string PageName, DateTime PubDate, bool IsNew, string Author);
}
