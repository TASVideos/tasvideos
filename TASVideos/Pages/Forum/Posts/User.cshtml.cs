using TASVideos.Data.Entity.Forum;
using static TASVideos.Pages.Forum.Topics.IndexModel;
namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UserModel(ApplicationDbContext db, IAwards awards, IPointsService pointsService) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	[FromQuery]
	public TopicRequest Search { get; set; } = new();

	public PageOf<UserPagePost, TopicRequest> Posts { get; set; } = new([], new());

	public async Task<IActionResult> OnGet()
	{
		var user = await db.Users
			.ForUser(UserName)
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
				u.BannedUntil,
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

		Posts = await db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.ExcludeRestricted(UserCanSeeRestricted)
			.OrderByDescending(p => p.CreateTimestamp)
			.Select(p => new UserPagePost
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				EnableHtml = p.EnableHtml,
				EnableBbCode = p.EnableBbCode,
				Restricted = p.Topic!.Forum!.Restricted,
				Text = p.Text,
				Subject = p.Subject,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name,
				PosterMood = p.PosterMood,
				PosterId = p.PosterId,
				PostEditedTimestamp = p.PostEditedTimestamp,
				TopicIsLocked = p.Topic!.IsLocked
			})
			.PageOf(Search);

		// Fill user data into each post
		var userAwards = (await awards.ForUser(user.Id)).ToList();

		var (points, rank) = await pointsService.PlayerPoints(user.Id);

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
			post.PosterPlayerRank = rank;
			post.PosterPronouns = user.PreferredPronouns;
			post.Awards = userAwards;
			post.PosterIsBanned = user.BannedUntil.HasValue && user.BannedUntil > DateTime.UtcNow;

			var isOwnPost = post.PosterId == User.GetUserId();
			var isOpenTopic = !post.TopicIsLocked;
			post.IsEditable = User.Has(PermissionTo.EditUsersForumPosts) || (isOwnPost && User.Has(PermissionTo.EditForumPosts) && isOpenTopic);

			// Note: IsLastPost is always false, because calculating it for every topic is too expensive, so we only check permissions
			// The goal here is for moderators to be able to modify posts from this screen, as a convenience
			post.IsDeletable = User.Has(PermissionTo.DeleteForumPosts);
		}

		return Page();
	}

	public class UserPagePost : PostEntry
	{
		public bool TopicIsLocked { get; init; }
		public string TopicTitle { get; init; } = "";
		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";
	}
}
