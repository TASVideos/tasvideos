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
			.Select(p => new TopicPost
			{
				Id = p.Id,
				EnableBbCode = p.EnableBbCode,
				EnableHtml = p.EnableHtml,
				Text = p.Text,
				Subject = p.Subject,
				PosterName = p.Poster!.UserName,
				CreateTimestamp = p.CreateTimestamp
			})
			.Take(l ?? 5)
			.ToListAsync();

		return View();
	}

	public class TopicPost
	{
		public int Id { get; init; }
		public bool EnableBbCode { get; init; }
		public bool EnableHtml { get; init; }
		public string Text { get; init; } = "";
		public string? Subject { get; init; }
		public string PosterName { get; init; } = "";
		public DateTime CreateTimestamp { get; init; }
	}
}
