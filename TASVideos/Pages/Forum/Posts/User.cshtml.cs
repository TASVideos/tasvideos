using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UserModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IAwards _awards;
	private readonly IPointsService _pointsService;

	public UserModel(
		ApplicationDbContext db,
		IAwards awards,
		IPointsService pointsService)
	{
		_db = db;
		_awards = awards;
		_pointsService = pointsService;
	}

	[FromRoute]
	public string UserName { get; set; } = "";

	[FromQuery]
	public UserPostsRequest Search { get; set; } = new();

	public PageOf<UserPagePost> Posts { get; set; } = PageOf<UserPagePost>.Empty();

	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = new List<AwardAssignmentSummary>();

	public async Task<IActionResult> OnGet()
	{
		var user = await _db.Users
			.Where(u => u.UserName == UserName)
			.Select(u => new
			{
				u.Id,
				u.UserName,
				Joined = u.CreateTimestamp,
				u.From,
				u.Avatar,
				u.MoodAvatarUrlBase,
				u.Signature,
				PostCount = u.Posts.Count,
				u.PreferredPronouns,
				Roles = u.UserRoles
					.Where(ur => !ur.Role!.IsDefault)
					.Select(ur => ur.Role!.Name)
					.ToList()
			})
			.SingleOrDefaultAsync();

		if (user is null)
		{
			return NotFound();
		}

		bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		Posts = await _db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.ExcludeRestricted(seeRestricted)
			.Select(p => new UserPagePost
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				EnableHtml = p.EnableHtml,
				EnableBbCode = p.EnableBbCode,
				Text = p.Text,
				Subject = p.Subject,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name,
				PosterMood = p.PosterMood
			})
			.SortedPageOf(Search);

		// Fill user data into each post
		var userAwards = (await _awards.ForUser(user.Id)).ToList();

		var (points, rank) = await _pointsService.PlayerPoints(user.Id);

		if (!string.IsNullOrWhiteSpace(rank))
		{
			user.Roles.Add(rank);
		}

		foreach (var post in Posts)
		{
			post.PosterName = user.UserName;
			post.Signature = user.Signature;
			post.PosterPostCount = user.PostCount;
			post.PosterLocation = user.From;
			post.PosterJoined = user.Joined;
			post.PosterPlayerPoints = points;
			post.PosterAvatar = user.Avatar;
			post.PosterMoodUrlBase = user.MoodAvatarUrlBase;
			post.PosterRoles = user.Roles;
			post.PosterPronouns = user.PreferredPronouns;
			post.Awards = userAwards;
		}

		return Page();
	}
}
