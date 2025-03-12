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
				wp.Revision,
				wp.RevisionMessage ?? "",
				wp.PageName,
				wp.LastUpdateTimestamp,
				wp.Revision == 1,
				wp.Author!.UserName))
			.Take(10)
			.ToListAsync();
		return Rss();
	}

	public record RssWiki(int RevisionId, string RevisionMessage, string PageName, DateTime PubDate, bool IsNew, string Author);
}
