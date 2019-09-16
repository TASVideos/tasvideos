using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
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
	}
}
