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
	private readonly IAwards _awards;

	public NewModel(
		ApplicationDbContext db,
		UserManager userManager,
		IAwards awards)
	{
		_db = db;
		_userManager = userManager;
		_awards = awards;
	}

	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<PostsSinceLastVisitModel> Posts { get; set; } = PageOf<PostsSinceLastVisitModel>.Empty();

	public async Task OnGet()
	{
		var user = await _userManager.GetUserAsync(User);
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var since = user.LastLoggedInTimeStamp ?? DateTime.UtcNow;
		Posts = await _db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Since(since)
			.Select(p => new PostsSinceLastVisitModel
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				EnableBbCode = p.EnableBbCode,
				EnableHtml = p.EnableHtml,
				Text = p.Text,
				Subject = p.Subject,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name,
				PosterId = p.PosterId,
				PosterName = p.Poster!.UserName,
				PosterRoles = p.Poster.UserRoles
					.Where(ur => !ur.Role!.IsDefault)
					.Where(ur => ur.Role!.Name != "Published Author")
					.Select(ur => ur.Role!.Name)
					.ToList(),
				PosterLocation = p.Poster.From,
				Signature = p.Poster.Signature,
				PosterAvatar = p.Poster.Avatar,
				PosterJoined = p.Poster.CreateTimestamp,
				PosterPostCount = p.Poster.Posts.Count,
				PosterMoodUrlBase = p.Poster.MoodAvatarUrlBase,
				PosterMood = p.PosterMood,
				PosterPronouns = p.Poster.PreferredPronouns
			})
			.OrderBy(p => p.CreateTimestamp)
			.PageOf(Search);

		foreach (var post in Posts)
		{
			post.Awards = await _awards.ForUser(post.PosterId);
		}
	}
}
