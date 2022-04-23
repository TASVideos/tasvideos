using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public LatestModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestPostsModel> Posts { get; set; } = PageOf<LatestPostsModel>.Empty();

	public async Task OnGet()
	{
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		Posts = await _db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Where(p => p.CreateTimestamp > DateTime.UtcNow.AddDays(-3))
			.Select(p => new LatestPostsModel
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				Text = p.Text,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name,
				PosterName = p.Poster!.UserName
			})
			.OrderByDescending(p => p.CreateTimestamp)
			.PageOf(Search);
	}
}
