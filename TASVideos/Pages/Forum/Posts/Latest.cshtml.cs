﻿using TASVideos.Core;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestPost> Posts { get; set; } = PageOf<LatestPost>.Empty();

	public async Task OnGet()
	{
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		Posts = await db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Since(DateTime.UtcNow.AddDays(-3))
			.OrderByDescending(p => p.CreateTimestamp)
			.ToLatestPost()
			.PageOf(Search);
	}

	public record LatestPost(
		DateTime CreateTimestamp,
		int Id,
		int TopicId,
		string TopicTitle,
		int ForumId,
		string ForumName,
		string Text,
		string PosterName);
}
