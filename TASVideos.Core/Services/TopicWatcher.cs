using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services
{
	public interface ITopicWatcher
	{
		/// <summary>
		/// Returns all topics the user is currently watching.
		/// </summary>
		Task<IEnumerable<WatchedTopic>> UserWatches(int userId);

		/// <summary>
		/// Notifies everyone watching a topic (other than the poster)
		/// that a new post has been created.
		/// </summary>
		/// <param name="notification">The data necessary to create a topic notification.</param>
		Task NotifyNewPost(TopicNotification notification);

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

	internal class TopicWatcher : ITopicWatcher
	{
		private readonly IEmailService _emailService;
		private readonly ApplicationDbContext _db;
		private readonly string _baseUrl;

		public TopicWatcher(
			IEmailService emailService,
			ApplicationDbContext db,
			AppSettings appSettings)
		{
			_emailService = emailService;
			_db = db;
			_baseUrl = appSettings.BaseUrl;
		}

		public async Task<IEnumerable<WatchedTopic>> UserWatches(int userId)
		{
			return await _db.ForumTopicWatches
				.ForUser(userId)
				.Select(tw => new WatchedTopic(
					tw.ForumTopic!.CreateTimestamp,
					tw.IsNotified,
					tw.ForumTopic.ForumId,
					tw.ForumTopic!.Forum!.Name,
					tw.ForumTopicId,
					tw.ForumTopic!.Title))
				.ToListAsync();
		}

		public async Task NotifyNewPost(TopicNotification notification)
		{
			var watches = await _db.ForumTopicWatches
				.Include(w => w.User)
				.Where(w => w.ForumTopicId == notification.TopicId)
				.Where(w => w.UserId != notification.PosterId)
				.Where(w => !w.IsNotified)
				.ToListAsync();

			if (watches.Any())
			{
				await _emailService
					.TopicReplyNotification(
						watches.Select(w => w.User!.Email),
						new TopicReplyNotificationTemplate(
							notification.PostId,
							notification.TopicId,
							notification.TopicTitle,
							_baseUrl));

				foreach (var watch in watches)
				{
					watch.IsNotified = true;
				}

				await _db.SaveChangesAsync();
			}
		}

		public async Task MarkSeen(int topicId, int userId)
		{
			var watchedTopic = await _db.ForumTopicWatches
				.SingleOrDefaultAsync(w => w.UserId == userId && w.ForumTopicId == topicId);

			if (watchedTopic is not null && watchedTopic.IsNotified)
			{
				watchedTopic.IsNotified = false;
				await _db.SaveChangesAsync();
			}
		}

		public async Task WatchTopic(int topicId, int userId, bool canSeeRestricted)
		{
			var topic = await _db.ForumTopics
				.ExcludeRestricted(canSeeRestricted)
				.SingleOrDefaultAsync(t => t.Id == topicId);

			if (topic is null)
			{
				return;
			}

			var watch = await _db.ForumTopicWatches
				.ExcludeRestricted(canSeeRestricted)
				.SingleOrDefaultAsync(w => w.UserId == userId
					&& w.ForumTopicId == topicId);

			if (watch is null)
			{
				_db.ForumTopicWatches.Add(new ForumTopicWatch
				{
					UserId = userId,
					ForumTopicId = topicId
				});

				await _db.SaveChangesAsync();
			}
		}

		public async Task UnwatchTopic(int topicId, int userId)
		{
			var watch = await _db.ForumTopicWatches
				.SingleOrDefaultAsync(w => w.UserId == userId
					&& w.ForumTopicId == topicId);

			if (watch is not null)
			{
				_db.ForumTopicWatches.Remove(watch);

				try
				{
					await _db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					// Do nothing
					// 1) if a watch is already removed, we are done
					// 2) if a watch was updated (for instance, someone posted in the topic),
					//        there isn't much we can do other than reload the page anyway with an error
					//        An error would only be modestly helpful anyway, and wouldn't save clicks
					//        However, this would be an nice to have one day
				}
			}
		}

		public async Task UnwatchAllTopics(int userId)
		{
			var watches = await _db.ForumTopicWatches
				.Where(w => w.UserId == userId)
				.ToListAsync();
			_db.ForumTopicWatches.RemoveRange(watches);
			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				// Do nothing
				// See UnwatchTopic for why
			}
		}

		public async Task<bool> IsWatchingTopic(int topicId, int userId)
		{
			return (await _db.ForumTopicWatches
				.SingleOrDefaultAsync(w => w.UserId == userId && w.ForumTopicId == topicId)) is not null;
		}
	}

	/// <summary>
	/// Represents a watched forum topic
	/// </summary>
	public record WatchedTopic(
		DateTime TopicCreateTimestamp,
		bool IsNotified,
		int ForumId,
		string ForumTitle,
		int TopicId,
		string TopicTitle);

	/// <summary>
	/// Represents a notification that a new post has been added to a topic
	/// </summary>
	public record TopicNotification(
		int PostId,
		int TopicId,
		string TopicTitle,
		int PosterId);
}
