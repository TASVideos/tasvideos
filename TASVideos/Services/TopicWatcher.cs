using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Services.Email;

namespace TASVideos.Services
{
	public interface ITopicWatcher
	{
		/// <summary>
		/// Returns all topics the user is currently watching
		/// </summary>
		Task<IEnumerable<WatchedTopic>> UserWatches(int userId);

		/// <summary>
		/// Notifies everyone watching a topic (other than the poster)
		/// that a new post has been created
		/// </summary>
		/// <param name="notification">The data necessary to create a topic notification</param>
		Task NotifyNewPost(TopicNotification notification);

		/// <summary>
		/// Marks that a user has seen a topic with new posts
		/// </summary>
		Task MarkSeen(int topicId, int userId);

		/// <summary>
		/// Allows a user to watch a topic
		/// </summary>
		Task WatchTopic(int topicId, int userId, bool canSeeRestricted);

		/// <summary>
		/// Removes a topic from the user's watched topic list
		/// </summary>
		Task UnwatchTopic(int topicId, int userId);
	}

	public class TopicWatcher : ITopicWatcher
	{
		private readonly IEmailService _emailService;
		private readonly ApplicationDbContext _db;

		public TopicWatcher(
			IEmailService emailService,
			ApplicationDbContext db)
		{
			_emailService = emailService;
			_db = db;
		}

		public async Task<IEnumerable<WatchedTopic>> UserWatches(int userId)
		{
			return await _db
				.ForumTopicWatches
				.ForUser(userId)
				.Select(tw => new WatchedTopic
				{
					TopicCreateTimeStamp = tw.ForumTopic!.CreateTimeStamp,
					IsNotified = tw.IsNotified,
					ForumId = tw.ForumTopic.ForumId,
					ForumTitle = tw.ForumTopic!.Forum!.Name,
					TopicId = tw.ForumTopicId,
					TopicTitle = tw.ForumTopic!.Title,
				})
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
						new TopicReplyNotificationTemplate
						{
							PostId = notification.PostId,
							TopicId = notification.TopicId,
							TopicTitle = notification.TopicTitle,
							BaseUrl = notification.BaseUrl
						});

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

			if (watchedTopic != null && watchedTopic.IsNotified)
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

			if (topic == null)
			{
				return;
			}

			var watch = await _db.ForumTopicWatches
				.ExcludeRestricted(canSeeRestricted)
				.SingleOrDefaultAsync(w => w.UserId == userId
					&& w.ForumTopicId == topicId);

			if (watch == null)
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

			if (watch != null)
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
					//		there isn't much we can do other than reload the page anyway with an error
					//		An error would only be modestly helpful anyway, and wouldn't save clicks
					//		However, this would be an nice to have one day
				}
			}
		}
	}
}
