using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[RequirePermission(
	true,
	PermissionTo.SeeRestrictedForums,
	PermissionTo.CreateForumPosts,
	PermissionTo.EditForumPosts,
	PermissionTo.DeleteForumPosts,
	PermissionTo.EditUsersForumPosts)]
public class EditModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IForumService forumService,
	IUserManager userManager)
	: BaseForumModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public ForumPostEditModel Post { get; set; } = new();

	public bool IsFirstPost { get; set; }
	public List<CreateModel.MiniPost> PreviousPosts { get; set; } = [];

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public async Task<IActionResult> OnGet()
	{
		var post = await db.ForumPosts
			.ExcludeRestricted(UserCanSeeRestricted)
			.Where(p => p.Id == Id)
			.Select(p => new ForumPostEditModel
			{
				CreateTimestamp = p.CreateTimestamp,
				PosterId = p.PosterId,
				PosterName = p.Poster!.UserName,
				EnableBbCode = p.EnableBbCode,
				EnableHtml = p.EnableHtml,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				Subject = p.Subject,
				Text = p.Text,
				Mood = p.PosterMood
			})
			.SingleOrDefaultAsync();

		if (post is null)
		{
			return NotFound();
		}

		if (!CanEditPost(post.PosterId))
		{
			return AccessDenied();
		}

		Post = post;
		var firstPostId = await db.ForumPosts
			.ForTopic(Post.TopicId)
			.OldestToNewest()
			.Select(p => p.Id)
			.FirstAsync();

		IsFirstPost = Id == firstPostId;

		PreviousPosts = await db.ForumPosts
			.ForTopic(Post.TopicId)
			.Where(fp => fp.CreateTimestamp < Post.CreateTimestamp)
			.ByMostRecent()
			.Select(fp => new CreateModel.MiniPost(
				fp.CreateTimestamp,
				fp.Poster!.UserName,
				fp.Poster.PreferredPronouns,
				fp.Text,
				fp.EnableBbCode,
				fp.EnableHtml))
			.Take(10)
			.Reverse()
			.ToListAsync();

		if (Post.PosterId == User.GetUserId())
		{
			UserAvatars = await forumService.UserAvatars(User.GetUserId());
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			if (Post.PosterId == User.GetUserId())
			{
				UserAvatars = await forumService.UserAvatars(User.GetUserId());
			}

			return Page();
		}

		var forumPost = await db.ForumPosts
			.Include(p => p.Topic)
			.Include(p => p.Topic!.Forum)
			.ExcludeRestricted(UserCanSeeRestricted)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (forumPost is null)
		{
			return NotFound();
		}

		if (!CanEditPost(forumPost.PosterId))
		{
			ModelState.AddModelError("", "Unable to edit post.");
			return Page();
		}

		if (!string.IsNullOrWhiteSpace(Post.TopicTitle))
		{
			var firstPostId = await db.ForumPosts
				.ForTopic(forumPost.Topic!.Id)
				.OldestToNewest()
				.Select(p => p.Id)
				.FirstAsync();
			if (Id == firstPostId)
			{
				forumPost.Topic!.Title = Post.TopicTitle;
			}
		}

		forumPost.Subject = Post.Subject;
		forumPost.Text = Post.Text;
		forumPost.PosterMood = Post.Mood;
		forumPost.PostEditedTimestamp = DateTime.UtcNow;

		var result = await db.TrySaveChanges();
		SetMessage(result, $"Post {Id} edited", "Unable to edit post");
		if (result.IsSuccess())
		{
			forumService.CacheEditedPostActivity(forumPost.ForumId, forumPost.Topic!.Id, forumPost.Id, (DateTime)forumPost.PostEditedTimestamp);
			await publisher.SendForum(
				forumPost.Topic!.Forum!.Restricted,
				$"[Post]({{0}}) edited by {User.Name()}",
				$"{forumPost.Topic.Forum.ShortName}: {forumPost.Topic.Title}",
				$"Forum/Posts/{Id}");
		}

		return BaseRedirect($"/Forum/Posts/{Id}");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var post = await db.ForumPosts
			.ExcludeRestricted(UserCanSeeRestricted)
			.Select(p => new
			{
				p.Id,
				p.TopicId,
				p.Topic!.Forum!.Restricted,
				p.Topic!.ForumId,
				ForumShortName = p.Topic!.Forum!.ShortName,
				TopicTitle = p.Topic!.Title
			})
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (post is null)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.DeleteForumPosts))
		{
			// Check if last post
			var lastPost = db.ForumPosts
				.ForTopic(post.TopicId ?? -1)
				.ByMostRecent()
				.First();

			bool isLastPost = lastPost.Id == post.Id;
			if (!isLastPost)
			{
				return NotFound();
			}
		}

		var postCount = await db.ForumPosts.CountAsync(t => t.TopicId == post.TopicId);

		await db.ForumPosts.Where(p => p.Id == Id).ExecuteDeleteAsync();

		bool topicDeleted = false;
		if (postCount == 1)
		{
			await db.ForumTopics.Where(t => t.Id == post.TopicId).ExecuteDeleteAsync();
			topicDeleted = true;
		}

		SuccessStatusMessage($"Post {Id} deleted");
		forumService.ClearLatestPostCache();
		forumService.ClearTopicActivityCache();
		await publisher.SendForum(
			post.Restricted,
			$"[{(topicDeleted ? "Topic" : "Post")} DELETED]({{0}}) by {User.Name()}",
			$"{post.ForumShortName}: {post.TopicTitle}",
			topicDeleted ? "" : $"Forum/Topics/{post.TopicId}");

		return topicDeleted
			? BasePageRedirect("/Forum/Subforum/Index", new { id = post.ForumId })
			: BasePageRedirect("/Forum/Topics/Index", new { id = post.TopicId });
	}

	public async Task<IActionResult> OnPostSpam()
	{
		if (!User.Has(PermissionTo.DeleteForumPosts) || !User.Has(PermissionTo.AssignRoles))
		{
			return AccessDenied();
		}

		var post = await db.ForumPosts
			.Where(p => p.Id == Id)
			.ExcludeRestricted(seeRestricted: false) // Intentionally not allowing spamming on restricted forums
			.Select(p => new
			{
				p.TopicId,
				TopicTitle = p.Topic!.Title,
				p.Topic!.ForumId,
				ForumShortName = p.Topic!.Forum!.ShortName,
				p.PosterId,
				PosterName = p.Poster!.UserName,
				PosterCannotBeSpammed = p.Poster!.UserRoles.SelectMany(ur => ur.Role!.RolePermission).Any(rp => rp.PermissionId == PermissionTo.AssignRoles)
			})
			.SingleOrDefaultAsync();

		if (post is null)
		{
			return NotFound();
		}

		if (post.PosterCannotBeSpammed)
		{
			return AccessDenied();
		}

		var postCount = await db.ForumPosts.CountAsync(p => p.TopicId == post.TopicId);

		await db.ForumPosts.Where(p => p.Id == Id)
			.ExecuteUpdateAsync(b => b
				.SetProperty(p => p.TopicId, SiteGlobalConstants.SpamTopicId)
				.SetProperty(p => p.ForumId, SiteGlobalConstants.SpamForumId));

		bool topicDeleted = false;
		if (postCount == 1)
		{
			await db.ForumTopics.Where(t => t.Id == post.TopicId).ExecuteDeleteAsync();
			topicDeleted = true;
		}

		SuccessStatusMessage($"Post {Id} marked as spam");
		forumService.ClearLatestPostCache();
		forumService.ClearTopicActivityCache();
		await userManager.PermaBanUser(post.PosterId);
		await publisher.SendForum(
			true,
			$"[{(topicDeleted ? "Topic" : "Post")} DELETED as SPAM]({{0}}), and user {post.PosterName} banned by {User.Name()}",
			$"{post.ForumShortName}: {post.TopicTitle}",
			topicDeleted ? "" : $"Forum/Topics/{post.TopicId}");

		return topicDeleted
			? BasePageRedirect("/Forum/Subforum/Index", new { id = post.ForumId })
			: BasePageRedirect("/Forum/Topics/Index", new { id = post.TopicId });
	}

	private bool CanEditPost(int posterId) => User.Has(PermissionTo.EditUsersForumPosts)
		|| (User.Has(PermissionTo.EditForumPosts) && posterId == User.GetUserId());

	public class ForumPostEditModel
	{
		public int PosterId { get; init; }
		public string PosterName { get; init; } = "";
		public DateTime CreateTimestamp { get; init; }
		public bool EnableBbCode { get; init; }
		public bool EnableHtml { get; init; }
		public int TopicId { get; init; }

		[StringLength(500)]
		public string TopicTitle { get; init; } = "";

		[StringLength(150)]
		public string? Subject { get; init; }
		public string Text { get; init; } = "";
		public string OriginalText => Text;
		public ForumPostMood Mood { get; init; } = ForumPostMood.Normal;
	}
}
