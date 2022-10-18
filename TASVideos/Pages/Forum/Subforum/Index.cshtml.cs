﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum;

[AllowAnonymous]
[RequireCurrentPermissions]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IForumService _forumService;

	public IndexModel(ApplicationDbContext db, IForumService forumService)
	{
		_db = db;
		_forumService = forumService;
	}

	[FromQuery]
	public ForumRequest Search { get; set; } = new();

	[FromRoute]
	public int Id { get; set; }

	public ForumDisplayModel Forum { get; set; } = new();
	public Dictionary<int, (string, string)> ActivityTopics { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var forum = await _db.Forums
			.ExcludeRestricted(seeRestricted)
			.Select(f => new ForumDisplayModel
			{
				Id = f.Id,
				Name = f.Name,
				Description = f.Description
			})
			.SingleOrDefaultAsync(f => f.Id == Id);

		if (forum is null)
		{
			return NotFound();
		}

		Forum = forum;
		Forum.Topics = await _db.ForumTopics
			.ForForum(Id)
			.Select(ft => new ForumDisplayModel.ForumTopicEntry
			{
				Id = ft.Id,
				Title = ft.Title,
				CreateUserName = ft.Poster!.UserName,
				CreateTimestamp = ft.CreateTimestamp,
				Type = ft.Type,
				IsLocked = ft.IsLocked,
				PostCount = ft.ForumPosts.Count,
				LastPost = ft.ForumPosts.FirstOrDefault(fp => fp.Id == ft.ForumPosts.Max(fpp => fpp.Id))
			})
			.OrderByDescending(ft => ft.Type)
			.ThenByDescending(ft => ft.LastPost != null ? ft.LastPost.Id : 0)
			.PageOf(Search);

		ActivityTopics = await _forumService.GetPostActivityOfSubforum(Id);

		return Page();
	}
}
