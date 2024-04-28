using System.ComponentModel;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[RequirePermission(PermissionTo.CreateForumPosts)]
public class CreateModel(
	UserManager userManager,
	ExternalMediaPublisher publisher,
	ApplicationDbContext db,
	ITopicWatcher topicWatcher,
	ILogger<CreateModel> logger,
	IForumService forumService)
	: BaseForumModel
{
	[FromRoute]
	public int TopicId { get; set; }

	[FromQuery]
	public int? QuoteId { get; set; }

	[BindProperty]
	public ForumPostCreateModel Post { get; set; } = new();

	[BindProperty]
	[DisplayName("Watch Topic for Replies")]
	public bool WatchTopic { get; set; }

	public List<MiniPost> PreviousPosts { get; set; } = [];

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var post = await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == TopicId)
			.Select(t => new ForumPostCreateModel
			{
				TopicTitle = t.Title,
				IsLocked = t.IsLocked
			})
			.SingleOrDefaultAsync();

		if (post is null)
		{
			return NotFound();
		}

		Post = post;
		if (Post.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
		{
			return AccessDenied();
		}

		if (QuoteId.HasValue)
		{
			var qPost = await db.ForumPosts
				.Include(p => p.Poster)
				.Where(p => p.Id == QuoteId)
				.SingleOrDefaultAsync();

			if (qPost is not null)
			{
				Post.Text = $"[quote=\"[post={QuoteId}][/post] {qPost.Poster!.UserName}\"]{qPost.Text}[/quote]";
			}
		}

		WatchTopic = await topicWatcher.IsWatchingTopic(TopicId, User.GetUserId());

		// Override default behavior if user setting demands it, even if they were already watching the topic
		var user = await userManager.GetRequiredUser(User);
		if (user.AutoWatchTopic is not null && user.AutoWatchTopic != UserPreference.Auto)
		{
			WatchTopic = user.AutoWatchTopic == UserPreference.Always;
		}

		PreviousPosts = await db.ForumPosts
			.ForTopic(TopicId)
			.OrderByDescending(fp => fp.CreateTimestamp)
			.Select(fp => new MiniPost(
				fp.CreateTimestamp,
				fp.Poster!.UserName,
				fp.Poster.PreferredPronouns,
				fp.Text,
				fp.EnableBbCode,
				fp.EnableHtml))
			.Take(10)
			.Reverse()
			.ToListAsync();

		UserAvatars = await forumService.UserAvatars(User.GetUserId());

		BackupSubmissionDeterminator = (await db.ForumPosts
			.ForTopic(TopicId)
			.Where(fp => fp.PosterId == user.Id)
			.CountAsync())
			.ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await userManager.GetRequiredUser(User);
		if (!ModelState.IsValid)
		{
			// We have to consider direct posting to this call, including "over-posting",
			// so all of this logic is necessary
			var isLocked = await forumService.IsTopicLocked(TopicId);
			if (isLocked && !User.Has(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			Post = new ForumPostCreateModel
			{
				TopicTitle = Post.TopicTitle,
				Subject = Post.Subject,
				Text = Post.Text,
				IsLocked = isLocked,
				Mood = User.Has(PermissionTo.UseMoodAvatars) ? Post.Mood : ForumPostMood.Normal
			};

			UserAvatars = new AvatarUrls(user.Avatar, user.MoodAvatarUrlBase);

			return Page();
		}

		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.SingleOrDefaultAsync(t => t.Id == TopicId);

		if (topic is null)
		{
			return NotFound();
		}

		if (topic.Forum!.Restricted && !User.Has(PermissionTo.SeeRestrictedForums))
		{
			return NotFound();
		}

		if (topic.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
		{
			return AccessDenied();
		}

		var id = await forumService.CreatePost(new PostCreateDto(
			topic.ForumId,
			TopicId,
			Post.Subject,
			Post.Text,
			user.Id,
			user.UserName,
			Post.Mood,
			IpAddress,
			WatchTopic));

		var mood = Post.Mood != ForumPostMood.Normal ? $" (Mood: {Post.Mood})" : "";
		var subject = string.IsNullOrWhiteSpace(Post.Subject) ? "" : $" ({Post.Subject})";
		if (TopicId == ForumConstants.NewsTopicId)
		{
			await publisher.AnnounceNewsPost(
				$"[News Post]({{0}}) by {user.UserName}{mood}",
				$"{topic.Forum.ShortName}: {topic.Title}{subject}",
				id);
		}
		else
		{
			await publisher.SendForum(
				topic.Forum.Restricted,
				$"[New Post]({{0}}) by {user.UserName}{mood}",
				$"{topic.Forum.ShortName}: {topic.Title}{subject}",
				$"Forum/Posts/{id}");
		}

		await userManager.AssignAutoAssignableRolesByPost(user.Id);

		try
		{
			await topicWatcher.NotifyNewPost(id, topic.Id, topic.Title, user.Id);
		}
		catch
		{
			// emails are currently somewhat unstable
			// we want to continue the request even if the email fails, so eat the exception
			logger.LogWarning("Email notification failed on new reply creation");
		}

		return BaseRedirect($"/Forum/Posts/{id}");
	}

	public class ForumPostCreateModel
	{
		public bool IsLocked { get; init; }
		public string TopicTitle { get; init; } = "";

		[StringLength(150)]
		public string? Subject { get; init; }
		public string Text { get; set; } = "";
		public ForumPostMood Mood { get; init; } = ForumPostMood.Normal;
	}

	public record MiniPost(
		DateTime CreateTimestamp,
		string PosterName,
		PreferredPronounTypes PosterPronouns,
		string Text,
		bool EnableHtml,
		bool EnableBbCode);
}
