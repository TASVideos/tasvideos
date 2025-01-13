using TASVideos.Data.Entity.Forum;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.TopicFeed)]
public class TopicFeed(ApplicationDbContext db) : WikiViewComponent
{
	public string? Heading { get; set; }
	public bool RightAlign { get; set; }
	public bool HideContent { get; set; }
	public string? WikiLink { get; set; }
	public List<TopicPost> Posts { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? l, int t, bool right, string? heading, bool hideContent, string wikiLink)
	{
		Heading = heading;
		RightAlign = right;
		HideContent = hideContent;
		WikiLink = wikiLink;

		Posts = await db.ForumPosts
			.ForTopic(t)
			.ExcludeRestricted(false) // By design, let's not allow restricted topics as wiki feeds
			.ByMostRecent()
			.Select(p => new TopicPost(
				p.Id,
				p.EnableBbCode,
				p.EnableHtml,
				p.Text,
				p.Subject,
				p.Poster!.UserName,
				p.CreateTimestamp))
			.Take(l ?? 5)
			.ToListAsync();

		return View();
	}

	public record TopicPost(int Id, bool EnableBbCode, bool EnableHtml, string Text, string? Subject, string PosterName, DateTime CreateTimestamp);
}
