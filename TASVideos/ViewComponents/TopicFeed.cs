using AutoMapper;
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
	private readonly IMapper _mapper;

	public TopicFeed(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? l, int t, bool right, string? heading, bool hideContent)
	{
		int limit = l ?? 5;
		int topicId = t;

		var model = new TopicFeedModel
		{
			RightAlign = right,
			Heading = heading,
			HideContent = hideContent,
			Posts = await _mapper.ProjectTo<TopicFeedModel.TopicPost>(
					_db.ForumPosts
					.ForTopic(topicId)
					.ExcludeRestricted(false) // By design, let's not allow restricted topics as wiki feeds
					.ByMostRecent())
				.Take(limit)
				.ToListAsync()
		};

		return View(model);
	}
}
