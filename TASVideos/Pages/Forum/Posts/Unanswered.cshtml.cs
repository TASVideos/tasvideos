﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UnansweredModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public UnansweredModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<UnansweredPostsModel> Posts { get; set; } = PageOf<UnansweredPostsModel>.Empty();

	public async Task OnGet()
	{
		Posts = await _db.ForumTopics
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
			.Where(t => t.ForumPosts.Count == 1)
			.Select(t => new UnansweredPostsModel
			{
				ForumId = t.ForumId,
				ForumName = t.Forum!.Name,
				TopicId = t.Id,
				TopicName = t.Title,
				AuthorId = t.PosterId,
				AuthorName = t.Poster!.UserName,
				PostDate = t.CreateTimestamp
			})
			.OrderByDescending(t => t.PostDate)
			.PageOf(Search);
	}
}
