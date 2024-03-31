using TASVideos.Data.Entity.Forum;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.TopicFeed)]
public class TopicFeed(ApplicationDbContext db) : WikiViewComponent
{
	public TopicFeedModel Feed { get; set; } = new();

	public async Task<IViewComponentResult> InvokeAsync(int? l, int t, bool right, string? heading, bool hideContent, string wikiLink)
	{
		int limit = l ?? 5;
		int topicId = t;

		Feed = new TopicFeedModel
		{
			RightAlign = right,
			Heading = heading,
			HideContent = hideContent,
			WikiLink = wikiLink,
			Posts = await db.ForumPosts
				.ForTopic(topicId)
				.ExcludeRestricted(false) // By design, let's not allow restricted topics as wiki feeds
				.ByMostRecent()
				.Select(p => new TopicFeedModel.TopicPost
				{
					Id = p.Id,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					Text = p.Text,
					Subject = p.Subject,
					PosterName = p.Poster!.UserName,
					CreateTimestamp = p.CreateTimestamp
				})
				.Take(limit)
				.ToListAsync()
		};

		return View();
	}

	public class TopicFeedModel
	{
		public string? Heading { get; set; }
		public bool RightAlign { get; set; }
		public bool HideContent { get; set; }
		public string? WikiLink { get; set; }

		public IEnumerable<TopicPost> Posts { get; set; } = [];

		public class TopicPost
		{
			public int Id { get; set; }
			public bool EnableBbCode { get; set; }
			public bool EnableHtml { get; set; }
			public string Text { get; set; } = "";
			public string? Subject { get; set; }
			public string PosterName { get; set; } = "";
			public DateTime CreateTimestamp { get; set; }
		}
	}
}
