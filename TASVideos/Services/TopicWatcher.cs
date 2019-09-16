using System;
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
		/// Notifies everyone watching a topic (other than the poster)
		/// that a new post has been created
		/// </summary>
		/// <param name="notification">The data necessary to create a topic notification</param>
		Task NotifyNewPost(TopicNotification notification);

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

		public async Task NotifyNewPost(TopicNotification notification)
		{
			if (notification == null)
			{
				throw new ArgumentNullException($"{nameof(notification)} can not be null");
			}

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
						watches.Select(w => w.User.Email),
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

		public async Task WatchTopic(int topicId, int userId, bool canSeeRestricted)
		{
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
				await _db.SaveChangesAsync();
			}
		}
	}
}
