using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IAwards _awards;

	public LatestModel(
		ApplicationDbContext db,
		IAwards awards)
	{
		_db = db;
		_awards = awards;
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
			.OrderByDescending(p => p.CreateTimestamp)
			.PageOf(Search);

		foreach (var post in Posts)
		{
			post.Awards = await _awards.ForUser(post.PosterId);
		}
	}
}
