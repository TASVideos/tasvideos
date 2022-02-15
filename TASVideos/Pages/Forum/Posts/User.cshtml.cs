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

	public UserPostsModel UserPosts { get; set; } = new();

	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = new List<AwardAssignmentSummary>();

	public async Task<IActionResult> OnGet()
	{
		var userPosts = await _db.Users
			.Where(u => u.UserName == UserName)
			.Select(u => new UserPostsModel
			{
				Id = u.Id,
				UserName = u.UserName,
				Joined = u.CreateTimestamp,
				Location = u.From,
				Avatar = u.Avatar,
				Signature = u.Signature,
				Roles = u.UserRoles
					.Where(ur => !ur.Role!.IsDefault)
					.Select(ur => ur.Role!.Name)
					.ToList()
			})
			.SingleOrDefaultAsync();

		if (userPosts == null)
		{
			return NotFound();
		}

		UserPosts = userPosts;
		Awards = await _awards.ForUser(UserPosts.Id);

		var (points, rank) = await _pointsService.PlayerPoints(UserPosts.Id);
		UserPosts.PlayerPoints = points;
		if (!string.IsNullOrWhiteSpace(rank))
		{
			UserPosts.Roles.Add(rank);
		}

		bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		UserPosts.Posts = await _db.ForumPosts
			.Where(p => p.PosterId == UserPosts.Id)
			.ExcludeRestricted(seeRestricted)
			.Select(p => new UserPostsModel.Post
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				EnableHtml = p.EnableHtml,
				EnableBbCode = p.EnableBbCode,
				Text = p.Text,
				Subject = p.Subject,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name
			})
			.SortedPageOf(Search);

		return Page();
	}
}
