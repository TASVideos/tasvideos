using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[Authorize]
[RequireCurrentPermissions]
public class NewModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager _userManager;

	public NewModel(ApplicationDbContext db, UserManager userManager)
	{
		_db = db;
		_userManager = userManager;
	}

	[FromQuery]
	public NewPagingModel Search { get; set; } = new();

	public class NewPagingModel : PagingModel
	{
		public NewPagingModel()
		{
			PageSize = 25;
		}
	}

	public PageOf<LatestPostsModel> Posts { get; set; } = PageOf<LatestPostsModel>.Empty();

	public async Task OnGet()
	{
		var user = await _userManager.GetUserAsync(User);
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var since = user.LastLoggedInTimeStamp ?? DateTime.UtcNow;
		Posts = await _db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Since(since)
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
