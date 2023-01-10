﻿using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[RequirePermission(PermissionTo.CreateForumPosts)]
public class CreateModel : BaseForumModel
{
	private readonly UserManager _userManager;
	private readonly ExternalMediaPublisher _publisher;
	private readonly ApplicationDbContext _db;
	private readonly ITopicWatcher _topicWatcher;
	private readonly ILogger<CreateModel> _logger;
	private readonly IForumService _forumService;

	public CreateModel(
		UserManager userManager,
		ExternalMediaPublisher publisher,
		ApplicationDbContext db,
		ITopicWatcher topicWatcher,
		ILogger<CreateModel> logger,
		IForumService forumService)
	{
		_userManager = userManager;
		_publisher = publisher;
		_db = db;
		_topicWatcher = topicWatcher;
		_logger = logger;
		_forumService = forumService;
	}

	[FromRoute]
	public int TopicId { get; set; }

	[FromQuery]
	public int? QuoteId { get; set; }

	[BindProperty]
	public ForumPostCreateModel Post { get; set; } = new();

	[BindProperty]
	[DisplayName("Watch Topic for Replies")]
	public bool WatchTopic { get; set; }

	public IEnumerable<MiniPostModel> PreviousPosts { get; set; } = new List<MiniPostModel>();

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var post = await _db.ForumTopics
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
			var qPost = await _db.ForumPosts
				.Include(p => p.Poster)
				.Where(p => p.Id == QuoteId)
				.SingleOrDefaultAsync();

			if (qPost != null)
			{
				Post.Text = $"[quote=\"{qPost.Poster!.UserName}\"]{qPost.Text}[/quote]";
			}
		}

		WatchTopic = await _topicWatcher.IsWatchingTopic(TopicId, User.GetUserId());

		// Override default behavior if user setting demands it, even if they were already watching the topic
		var user = await _userManager.GetUserAsync(User);
		if (user.AutoWatchTopic != null && user.AutoWatchTopic != UserPreference.Auto)
		{
			WatchTopic = user.AutoWatchTopic == UserPreference.Always;
		}

		PreviousPosts = await _db.ForumPosts
			.ForTopic(TopicId)
			.Select(fp => new MiniPostModel
			{
				CreateTimestamp = fp.CreateTimestamp,
				PosterName = fp.Poster!.UserName,
				PosterPronouns = fp.Poster.PreferredPronouns,
				Text = fp.Text,
				EnableBbCode = fp.EnableBbCode,
				EnableHtml = fp.EnableHtml
			})
			.OrderByDescending(fp => fp.CreateTimestamp)
			.Take(10)
			.Reverse()
			.ToListAsync();

		UserAvatars = await _forumService.UserAvatars(User.GetUserId());

		BackupSubmissionDeterminator = (await _db.ForumPosts
			.ForTopic(TopicId)
			.Where(fp => fp.PosterId == user.Id)
			.CountAsync())
			.ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await _userManager.GetUserAsync(User);
		if (!ModelState.IsValid)
		{
			// We have to consider direct posting to this call, including "over-posting",
			// so all of this logic is necessary
			var isLocked = await _forumService.IsTopicLocked(TopicId);
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
				UserAvatar = user.Avatar,
				UserSignature = user.Signature,
				Mood = User.Has(PermissionTo.UseMoodAvatars) ? Post.Mood : ForumPostMood.Normal
			};

			UserAvatars = new AvatarUrls(user.Avatar, user.MoodAvatarUrlBase);

			return Page();
		}

		var topic = await _db.ForumTopics
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

		var id = await _forumService.CreatePost(new PostCreateDto(
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
			await _publisher.AnnounceForum(
				$"News Post by {user.UserName}{mood}",
				$"{topic.Forum.ShortName}: {topic.Title}{subject}",
				$"Forum/Posts/{id}");
		}
		else
		{
			await _publisher.SendForum(
				topic.Forum.Restricted,
				$"New Post by {user.UserName}{mood}",
				$"{topic.Forum.ShortName}: {topic.Title}{subject}",
				$"Forum/Posts/{id}");
		}

		await _userManager.AssignAutoAssignableRolesByPost(user.Id);

		try
		{
			await _topicWatcher.NotifyNewPost(new TopicNotification(
				id, topic.Id, topic.Title, user.Id));
		}
		catch
		{
			// emails are currently somewhat unstable
			// we want to continue the request even if the email fails, so eat the exception
			_logger.LogWarning("Email notification failed on new reply creation");
		}

		return BaseRedirect($"/Forum/Posts/{id}");
	}
}
