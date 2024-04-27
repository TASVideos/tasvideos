﻿using System.ComponentModel;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[RequirePermission(
	true,
	PermissionTo.SeeRestrictedForums,
	PermissionTo.CreateForumPosts,
	PermissionTo.DeleteForumPosts,
	PermissionTo.EditForumPosts)]
public class EditModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IForumService forumService,
	IRoleService roleService)
	: BaseForumModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public ForumPostEditModel Post { get; set; } = new();

	[BindProperty]
	[DisplayName("Minor Edit")]
	public bool MinorEdit { get; set; }
	public List<CreateModel.MiniPost> PreviousPosts { get; set; } = [];

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var post = await db.ForumPosts
			.ExcludeRestricted(seeRestricted)
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

		Post = post;
		var firstPostId = (await db.ForumPosts
			.ForTopic(Post.TopicId)
			.OldestToNewest()
			.FirstAsync())
			.Id;

		Post.IsFirstPost = Id == firstPostId;

		if (!User.Has(PermissionTo.EditForumPosts)
			&& Post.PosterId != User.GetUserId())
		{
			return AccessDenied();
		}

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
			MinorEdit = true;
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

		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

		var forumPost = await db.ForumPosts
			.Include(p => p.Topic)
			.Include(p => p.Topic!.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (forumPost is null)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.EditForumPosts)
			&& forumPost.PosterId != User.GetUserId())
		{
			ModelState.AddModelError("", "Unable to edit post.");
			return Page();
		}

		if (!string.IsNullOrWhiteSpace(Post.TopicTitle))
		{
			var firstPostId = (await db.ForumPosts
				.ForTopic(forumPost.Topic!.Id)
				.OldestToNewest()
				.FirstAsync())
				.Id;
			if (Id == firstPostId)
			{
				forumPost.Topic!.Title = Post.TopicTitle;
			}
		}

		forumPost.Subject = Post.Subject;
		forumPost.Text = Post.Text;
		forumPost.PosterMood = Post.Mood;
		forumPost.PostEditedTimestamp = DateTime.UtcNow;

		var result = await ConcurrentSave(db, $"Post {Id} edited", "Unable to edit post");
		if (result)
		{
			forumService.CacheEditedPostActivity(forumPost.ForumId, forumPost.Topic!.Id, forumPost.Id, (DateTime)forumPost.PostEditedTimestamp);

			if (!MinorEdit)
			{
				await publisher.SendForum(
					forumPost.Topic!.Forum!.Restricted,
					$"[Post]({{0}}) edited by {User.Name()}",
					$"{forumPost.Topic.Forum.ShortName}: {forumPost.Topic.Title}",
					$"Forum/Posts/{Id}");
			}
		}

		return BaseRedirect($"/Forum/Posts/{Id}");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var post = await db.ForumPosts
			.Include(p => p.Topic)
			.Include(p => p.Topic!.Forum)
			.ExcludeRestricted(seeRestricted)
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

		db.ForumPosts.Remove(post);

		bool topicDeleted = false;
		if (postCount == 1)
		{
			var topic = await db.ForumTopics.SingleAsync(t => t.Id == post.TopicId);
			db.ForumTopics.Remove(topic);
			topicDeleted = true;
		}

		var result = await ConcurrentSave(db, $"Post {Id} deleted", $"Unable to delete post {Id}");

		if (result)
		{
			forumService.ClearLatestPostCache();
			forumService.ClearTopicActivityCache();
			await publisher.SendForum(
				post.Topic!.Forum!.Restricted,
				$"[{(topicDeleted ? "Topic" : "Post")} DELETED]({{0}}) by {User.Name()}",
				$"{post.Topic!.Forum!.ShortName}: {post.Topic.Title}",
				topicDeleted ? "" : $"Forum/Topics/{post.Topic.Id}");
		}

		return topicDeleted
			? BasePageRedirect("/Forum/Subforum/Index", new { id = post.Topic!.ForumId })
			: BasePageRedirect("/Forum/Topics/Index", new { id = post.TopicId });
	}

	public async Task<IActionResult> OnPostSpam()
	{
		var post = await db.ForumPosts
			.Include(p => p.Poster)
			.ThenInclude(p => p!.UserRoles)
			.ThenInclude(ur => ur.Role)
			.Include(p => p.Topic)
			.Include(p => p.Topic!.Forum)
			.ExcludeRestricted(seeRestricted: false) // Intentionally not allowing spamming on restricted forums
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (post is null)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.DeleteForumPosts) || !User.Has(PermissionTo.AssignRoles))
		{
			return AccessDenied();
		}

		if (post.Poster!.UserRoles.SelectMany(ur => ur.Role!.RolePermission).Any(rp => rp.PermissionId == PermissionTo.AssignRoles))
		{
			return AccessDenied();
		}

		var postCount = await db.ForumPosts.CountAsync(t => t.TopicId == post.TopicId);

		var oldTopicId = post.TopicId;
		var oldForumId = post.Topic!.ForumId;
		var oldTopicTitle = post.Topic.Title;
		var oldForumShortName = post.Topic.Forum!.ShortName;
		post.TopicId = SiteGlobalConstants.SpamTopicId;
		post.ForumId = SiteGlobalConstants.SpamForumId;

		bool topicDeleted = false;
		if (postCount == 1)
		{
			var topic = await db.ForumTopics.SingleAsync(t => t.Id == oldTopicId);
			db.ForumTopics.Remove(topic);
			topicDeleted = true;
		}

		var result = await ConcurrentSave(db, $"Post {Id} deleted", $"Unable to delete post {Id}");

		if (result)
		{
			forumService.ClearLatestPostCache();
			forumService.ClearTopicActivityCache();
			await roleService.RemoveRolesFromUser(post.PosterId);
			await publisher.SendForum(
				true,
				$"[{(topicDeleted ? "Topic" : "Post")} DELETED as SPAM]({{0}}), and user {post.Poster!.UserName} banned by {User.Name()}",
				$"{oldForumShortName}: {oldTopicTitle}",
				topicDeleted ? "" : $"Forum/Topics/{oldTopicId}");
		}

		return topicDeleted
			? BasePageRedirect("/Forum/Subforum/Index", new { id = oldForumId })
			: BasePageRedirect("/Forum/Topics/Index", new { id = oldTopicId });
	}

	public class ForumPostEditModel
	{
		public int PosterId { get; init; }
		public string PosterName { get; init; } = "";
		public DateTime CreateTimestamp { get; init; }
		public bool EnableBbCode { get; init; }
		public bool EnableHtml { get; init; }
		public int TopicId { get; init; }

		[DisplayName("Topic Title")]
		[StringLength(500)]
		public string TopicTitle { get; init; } = "";

		[StringLength(150)]
		public string? Subject { get; init; }
		public string Text { get; init; } = "";
		public string OriginalText => Text;
		public bool IsFirstPost { get; set; }
		public ForumPostMood Mood { get; init; } = ForumPostMood.Normal;
	}
}
