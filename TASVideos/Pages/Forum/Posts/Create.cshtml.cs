using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[RequirePermission(PermissionTo.CreateForumPosts)]
public class CreateModel(
	IUserManager userManager,
	IExternalMediaPublisher publisher,
	ApplicationDbContext db,
	ITopicWatcher topicWatcher,
	IForumService forumService)
	: BaseForumModel
{
	[FromRoute]
	public int TopicId { get; set; }

	[FromQuery]
	public int? QuoteId { get; set; }

	public bool IsLocked { get; set; }
	public string TopicTitle { get; set; } = "";

	[BindProperty]
	[StringLength(150)]
	public string? Subject { get; set; }

	[BindProperty]
	public string Text { get; set; } = "";

	[BindProperty]
	public ForumPostMood Mood { get; set; } = ForumPostMood.Normal;

	[BindProperty]
	public bool WatchTopic { get; set; }

	public List<MiniPost> PreviousPosts { get; set; } = [];

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var topic = await db.ForumTopics
			.ExcludeRestricted(UserCanSeeRestricted)
			.Where(t => t.Id == TopicId)
			.Select(t => new { t.Title, t.IsLocked })
			.SingleOrDefaultAsync();

		if (topic is null)
		{
			return NotFound();
		}

		TopicTitle = topic.Title;
		IsLocked = topic.IsLocked;
		if (IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
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
				Text = $"[quote=\"[post={QuoteId}][/post] {qPost.Poster!.UserName}\"]{qPost.Text}[/quote]";
			}
		}

		WatchTopic = await topicWatcher.IsWatchingTopic(TopicId, User.GetUserId());

		var user = await userManager.GetRequiredUser(User);
		WatchTopic = user.AutoWatchTopic switch
		{
			UserPreference.Auto => WatchTopic,
			UserPreference.Always => true,
			UserPreference.Never => false,
			_ => WatchTopic,
		};

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
		BackupSubmissionDeterminator = (await forumService.GetPostCountInTopic(user.Id, TopicId)).ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await userManager.GetRequiredUser(User);
		if (!ModelState.IsValid)
		{
			var isLocked = await forumService.IsTopicLocked(TopicId);
			if (isLocked && !User.Has(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			IsLocked = isLocked;
			Mood = User.Has(PermissionTo.UseMoodAvatars) ? Mood : ForumPostMood.Normal;
			UserAvatars = new AvatarUrls(user.Avatar, user.MoodAvatarUrlBase);

			return Page();
		}

		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.SingleOrDefaultAsync(t => t.Id == TopicId);

		if (topic is null || (topic.Forum!.Restricted && !User.Has(PermissionTo.SeeRestrictedForums)))
		{
			return NotFound();
		}

		if (topic.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
		{
			return AccessDenied();
		}

		var id = await forumService.CreatePost(new PostCreate(
			topic.ForumId, TopicId, Subject, Text, user.Id, user.UserName, Mood, IpAddress, WatchTopic));

		var mood = Mood != ForumPostMood.Normal ? $" (Mood: {Mood})" : "";
		var subject = string.IsNullOrWhiteSpace(Subject) ? "" : $" ({Subject})";
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
		await topicWatcher.NotifyNewPost(id, topic.Id, topic.Title, user.Id);

		return BaseRedirect($"/Forum/Posts/{id}");
	}

	public record MiniPost(
		DateTime CreateTimestamp,
		string PosterName,
		PreferredPronounTypes PosterPronouns,
		string Text,
		bool EnableHtml,
		bool EnableBbCode);
}
