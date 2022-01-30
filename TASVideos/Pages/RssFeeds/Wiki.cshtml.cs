using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.RssFeeds.Models;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class WikiModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public WikiModel(ApplicationDbContext db, AppSettings settings)
	{
		_db = db;
		BaseUrl = settings.BaseUrl;
	}

	public List<RssWiki> WikiEdits { get; set; } = new();
	public string BaseUrl { get; set; }

	public async Task<IActionResult> OnGet()
	{
		WikiEdits = await _db.WikiPages
			.ByMostRecent()
			.Select(wp => new RssWiki
			{
				RevisionMessage = wp.RevisionMessage ?? "",
				PageName = wp.PageName,
				PubDate = wp.LastUpdateTimestamp,
				IsNew = wp.Revision == 1,
				Author = wp.CreateUserName ?? ""
			})
			.Take(10)
			.ToListAsync();
		PageResult pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
