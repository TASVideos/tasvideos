using Microsoft.Extensions.Logging;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services;

public interface ITopicWatcher
{
	/// <summary>
	/// Returns all topics the user is currently watching.
	/// </summary>
	Task<PageOf<WatchedTopic>> UserWatches(int userId, PagingModel paging);

	/// <summary>
	/// Notifies everyone watching a topic (other than the poster)
	/// that a new post has been created.
	/// </summary>
	Task NotifyNewPost(int postId, int topicId, string topicTitle, int posterId);

	/// <summary>
	/// Marks that a user has seen a topic with new posts.
	/// </summary>
	Task MarkSeen(int topicId, int userId);

	/// <summary>
	/// Allows a user to watch a topic.
	/// </summary>
	Task WatchTopic(int topicId, int userId, bool canSeeRestricted);

	/// <summary>
	/// Removes a topic from the user's watched topic list.
	/// </summary>
	Task UnwatchTopic(int topicId, int userId);

	/// <summary>
	/// Removes every topic from the user's watched topic list.
	/// </summary>
	Task UnwatchAllTopics(int userId);

	/// <summary>
	/// Returns whether the user watches a specific topic.
	/// </summary>
	Task<bool> IsWatchingTopic(int topicId, int userId);
}

internal class TopicWatcher(
	IEmailService emailService,
	ApplicationDbContext db,
	AppSettings appSettings,
	ILogger<TopicWatcher> logger)
	: ITopicWatcher
{
	private readonly string _baseUrl = appSettings.BaseUrl;

	public async Task<PageOf<WatchedTopic>> UserWatches(int userId, PagingModel paging)
	{
		return await db.ForumTopicWatches
			.ForUser(userId)
			.Select(tw => new WatchedTopic
			{
				LastPostedOn = tw.ForumTopic!.ForumPosts
					.Where(fp => fp.Id == tw.ForumTopic.ForumPosts.Max(fpp => fpp.Id))
					.Select(fp => fp.CreateTimestamp)
					.First(),
				IsNotified = tw.IsNotified,
				ForumId = tw.ForumTopic.ForumId,
				Forum = tw.ForumTopic!.Forum!.Name,
				TopicId = tw.ForumTopicId,
				Topic = tw.ForumTopic!.Title
			})
			.SortedPageOf(paging);
	}

	public async Task NotifyNewPost(int postId, int topicId, string topicTitle, int posterId)
	{
		var watches = await db.ForumTopicWatches
			.Include(w => w.User)
			.Where(w => w.ForumTopicId == topicId)
			.Where(w => w.UserId != posterId)
			.Where(w => !w.IsNotified)
			.ToListAsync();

		if (watches.Any())
		{
			try
			{
				await emailService.TopicReplyNotification(
					watches.Select(w => w.User!.Email),
					new TopicReplyNotificationTemplate(postId, topicId, topicTitle, _baseUrl));
			}
			catch
			{
				// emails are currently somewhat unstable
				// we want to continue the request even if the email fails, so eat the exception
				logger.LogWarning("Email notification failed on new reply creation");
			}

			foreach (var watch in watches)
			{
				watch.IsNotified = true;
			}

			await db.SaveChangesAsync();
		}
	}

	public async Task MarkSeen(int topicId, int userId)
	{
		await db.ForumTopicWatches
			.Where(w => w.UserId == userId && w.ForumTopicId == topicId)
			.ExecuteUpdateAsync(s => s.SetProperty(w => w.IsNotified, false));
	}

	public async Task WatchTopic(int topicId, int userId, bool canSeeRestricted)
	{
		var topicExists = await db.ForumTopics
			.ExcludeRestricted(canSeeRestricted)
			.AnyAsync(t => t.Id == topicId);

		if (!topicExists)
		{
			return;
		}

		var watchExists = await db.ForumTopicWatches
			.ExcludeRestricted(canSeeRestricted)
			.AnyAsync(w => w.UserId == userId
				&& w.ForumTopicId == topicId);

		if (watchExists)
		{
			return;
		}

		db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			UserId = userId,
			ForumTopicId = topicId
		});

		await db.SaveChangesAsync();
	}

	public async Task UnwatchTopic(int topicId, int userId)
	{
		await db.ForumTopicWatches
			.Where(w => w.UserId == userId && w.ForumTopicId == topicId)
			.ExecuteDeleteAsync();
	}

	public async Task UnwatchAllTopics(int userId)
	{
		await db.ForumTopicWatches
			.Where(w => w.UserId == userId)
			.ExecuteDeleteAsync();
	}

	public async Task<bool> IsWatchingTopic(int topicId, int userId)
		=> await db.ForumTopicWatches.AnyAsync(w => w.UserId == userId && w.ForumTopicId == topicId);
}

/// <summary>
/// Represents a watched forum topic
/// </summary>
public class WatchedTopic
{
	[Sortable]
	public string Forum { get; init; } = "";

	[TableIgnore]
	public int ForumId { get; init; }

	[Sortable]
	public string Topic { get; init; } = "";

	[TableIgnore]
	public int TopicId { get; init; }

	[Sortable]
	public DateTime LastPostedOn { get; init; }

	[TableIgnore]
	public bool IsNotified { get; init; }
}
