using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.TopicFeed)]
public class TopicFeed : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public TopicFeed(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? l, int t, bool right, string? heading, bool hideContent, string wikiLink)
	{
		int limit = l ?? 5;
		int topicId = t;

		var model = new TopicFeedModel
		{
			RightAlign = right,
			Heading = heading,
			HideContent = hideContent,
			WikiLink = wikiLink,
			Posts = await _db.ForumPosts
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

		return View(model);
	}
}
