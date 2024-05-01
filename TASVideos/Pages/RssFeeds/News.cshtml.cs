using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.RssFeeds;

[ResponseCache(Duration = 1200)]
public class NewsModel(ApplicationDbContext db) : BasePageModel
{
	private const int NewsTopicId = 8694; // Unfortunately, there is not an easy way to not hard code this

	public List<RssNews> News { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		News = await db.ForumPosts
			.ForTopic(NewsTopicId)
			.ByMostRecent()
			.Select(p => new RssNews(
				p.Id,
				p.LastUpdateTimestamp,
				p.Subject ?? "",
				p.Text,
				p.EnableHtml,
				p.EnableBbCode))
			.Take(10)
			.ToListAsync();
		return Rss();
	}

	public record RssNews(int PostId, DateTime PubDate, string Subject, string Text, bool EnableHtml, bool EnableBbCode);
}
