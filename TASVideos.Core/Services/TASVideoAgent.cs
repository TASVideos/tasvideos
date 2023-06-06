namespace TASVideos.Core.Services;

public interface ITASVideoAgent
{
	Task<int> PostSubmissionTopic(int submissionId, string postTitle);
	Task PostSubmissionPublished(int submissionId, int publicationId);
	Task PostSubmissionUnpublished(int submissionId);

	Task SendWelcomeMessage(int userId);
	Task SendAutoAssignedRole(int userId, string roleName);
	Task SendPublishedAuthorRole(int userId, string roleName, string publicationTitle);

}

internal class TASVideoAgent : ITASVideoAgent
{
	private readonly ApplicationDbContext _db;
	private readonly IForumService _forumService;

	public TASVideoAgent(ApplicationDbContext db, IForumService forumService)
	{
		_db = db;
		_forumService = forumService;
	}

	public async Task<int> PostSubmissionTopic(int submissionId, string title)
	{
		var poll = new ForumPoll
		{
			Question = SiteGlobalConstants.PollQuestion,
			PollOptions = new ForumPollOption[]
			{
					new () { Text = SiteGlobalConstants.PollOptionNo, Ordinal = 0 },
					new () { Text = SiteGlobalConstants.PollOptionYes, Ordinal = 1 },
					new () { Text = SiteGlobalConstants.PollOptionsMeh, Ordinal = 2 }
			}
		};

		// Create Topic in workbench
		var topic = new ForumTopic
		{
			ForumId = ForumConstants.WorkBenchForumId,
			Title = title,
			PosterId = SiteGlobalConstants.TASVideoAgentId,
			SubmissionId = submissionId,
			Poll = poll
		};

		// Create first post
		var post = new ForumPost
		{
			Topic = topic,
			ForumId = ForumConstants.WorkBenchForumId,
			PosterId = SiteGlobalConstants.TASVideoAgentId,
			Text = SiteGlobalConstants.NewSubmissionPost + $"[submission]{submissionId}[/submission]",
			EnableHtml = false,
			EnableBbCode = true,
			PosterMood = ForumPostMood.Normal
		};

		_db.ForumPolls.Add(poll);
		_db.ForumTopics.Add(topic);
		_db.ForumPosts.Add(post);
		await _db.SaveChangesAsync();

		poll.TopicId = topic.Id;
		await _db.SaveChangesAsync();

		_forumService.CacheLatestPost(
			ForumConstants.WorkBenchForumId,
			topic.Id,
			new LatestPost(post.Id, post.CreateTimestamp, SiteGlobalConstants.TASVideoAgent));
		_forumService.CacheNewPostActivity(post.ForumId, topic.Id, post.Id, post.CreateTimestamp);

		return topic.Id;
	}

	public async Task PostSubmissionPublished(int submissionId, int publicationId)
	{
		var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.SubmissionId == submissionId);

		// We intentionally silently fail here.
		// Otherwise we would leave publication in a partial state
		// which would be worse than a missing forum post
		if (topic is not null)
		{
			topic.ForumId = SiteGlobalConstants.PublishedMoviesForumId;
			var postsToMove = await _db.ForumPosts
				.ForTopic(topic.Id)
				.ToListAsync();
			foreach (var post in postsToMove)
			{
				post.ForumId = SiteGlobalConstants.PublishedMoviesForumId;
			}

			_db.ForumPosts.Add(new ForumPost
			{
				TopicId = topic.Id,
				ForumId = topic.ForumId,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				EnableBbCode = true,
				EnableHtml = false,
				Subject = SiteGlobalConstants.NewPublicationPostSubject,
				Text = SiteGlobalConstants.NewPublicationPost.Replace("{PublicationId}", publicationId.ToString()),
				PosterMood = ForumPostMood.Happy
			});
			await _db.SaveChangesAsync();

			_forumService.ClearLatestPostCache();
			_forumService.ClearTopicActivityCache();
		}
	}

	public async Task PostSubmissionUnpublished(int submissionId)
	{
		var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.SubmissionId == submissionId);

		// We intentionally silently fail here.
		// Otherwise we would leave publication in a partial state
		// which would be worse than a missing forum post
		if (topic is not null)
		{
			topic.ForumId = SiteGlobalConstants.WorkbenchForumId;
			var postsToMove = await _db.ForumPosts
				.ForTopic(topic.Id)
				.ToListAsync();
			foreach (var post in postsToMove)
			{
				post.ForumId = SiteGlobalConstants.WorkbenchForumId;
			}

			_db.ForumPosts.Add(new ForumPost
			{
				TopicId = topic.Id,
				ForumId = topic.ForumId,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				EnableBbCode = true,
				EnableHtml = false,
				Subject = SiteGlobalConstants.UnpublishSubject,
				Text = SiteGlobalConstants.UnpublishPost,
				PosterMood = ForumPostMood.Puzzled
			});
			await _db.SaveChangesAsync();

			_forumService.ClearLatestPostCache();
			_forumService.ClearTopicActivityCache();
		}
	}

	public Task SendWelcomeMessage(int userId)
	{
		return SendPm(userId, SiteGlobalConstants.WelcomeToTasvideosPostId, t => t);
	}

	public Task SendAutoAssignedRole(int userId, string roleName)
	{
		return SendPm(
			userId,
			SiteGlobalConstants.AutoAssignedRolePostId,
			t => t.Replace("[[role]]", roleName));
	}

	public Task SendPublishedAuthorRole(int userId, string roleName, string publicationTitle)
	{
		return SendPm(
			userId,
			SiteGlobalConstants.PublishedAuthorRoleAddedPostId,
			t => t.Replace("[[role]]", roleName).Replace("[[publicationTitle]]", publicationTitle));
	}

	private async Task SendPm(int userId, int postId, Func<string, string> processTemplate)
	{
		var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
		if (user is null)
		{
			return;
		}

		var post = await _db.ForumPosts.SingleOrDefaultAsync(p => p.Id == postId);
		if (post is null)
		{
			return;
		}

		_db.PrivateMessages.Add(new PrivateMessage
		{
			FromUserId = SiteGlobalConstants.TASVideoAgentId,
			ToUserId = user.Id,
			Subject = post.Subject,
			Text = processTemplate(post.Text).Replace("[[username]]", user.UserName),
			EnableBbCode = true
		});
		await _db.SaveChangesAsync();
	}
}
