using TASVideos.Core;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UserModel(ApplicationDbContext db, IAwards awards, IPointsService pointsService) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	[FromQuery]
	public UserPostsRequest Search { get; set; } = new();

	public PageOf<UserPagePost> Posts { get; set; } = PageOf<UserPagePost>.Empty();

	public List<AwardAssignmentSummary> Awards { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var user = await db.Users
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

		Posts = await db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
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
				PostEditedTimestamp = p.PostEditedTimestamp
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
		}

		return Page();
	}

	public class UserPostsRequest : PagingModel
	{
		public UserPostsRequest()
		{
			PageSize = ForumConstants.PostsPerPage;
			Sort = $"-{nameof(UserPagePost.CreateTimestamp)}";
		}
	}

	public class UserPagePost : IForumPostEntry
	{
		public int Id { get; init; }
		public DateTime CreateTimestamp { get; init; }
		public DateTime LastUpdateTimestamp { get; init; }
		public bool EnableBbCode { get; init; }
		public bool EnableHtml { get; init; }
		public bool Restricted { get; init; }
		public string Text { get; init; } = "";
		public DateTime? PostEditedTimestamp { get; init; }
		public string? Subject { get; init; }
		public int TopicId { get; init; }
		public string TopicTitle { get; init; } = "";
		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";
		public ForumPostMood PosterMood { get; init; }

		// Not needed
		public bool Highlight => false;
		public bool IsEditable => false;
		public bool IsDeletable => false;

		// Fill with user info
		public int PosterId { get; set; }
		public string PosterName { get; set; } = "";
		public string? Signature { get; set; }
		public int PosterPostCount { get; set; }
		public string? PosterLocation { get; set; }
		public DateTime PosterJoined { get; set; }
		public double PosterPlayerPoints { get; set; }
		public string? PosterAvatar { get; set; }
		public string? PosterMoodUrlBase { get; set; }
		public IList<string> PosterRoles { get; set; } = [];
		public string? PosterPlayerRank { get; set; }
		public PreferredPronounTypes PosterPronouns { get; set; }
		public ICollection<AwardAssignmentSummary> Awards { get; set; } = [];
	}
}
