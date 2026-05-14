using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class NewsModel(ApplicationDbContext db) : BasePageModel
{
	public List<RssNews> News { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		News = await db.ForumPosts
			.WhereSpecialContentType()
			.ByMostRecent()
			.Select(p => new RssNews(
				p.Id,
				p.LastUpdateTimestamp,
				p.Topic!.Title,
				p.Subject ?? "",
				p.Text,
				p.EnableHtml,
				p.EnableBbCode))
			.Take(10)
			.ToListAsync();
		return Rss();
	}

	public record RssNews(int PostId, DateTime PubDate, string Title, string Subject, string Text, bool EnableHtml, bool EnableBbCode);
}
