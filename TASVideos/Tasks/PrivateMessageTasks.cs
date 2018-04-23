using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class PrivateMessageTasks
	{
		private readonly string _messageCountCacheKey = $"{nameof(ForumTasks)}-{nameof(GetUnreadMessageCount)}-";

		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PrivateMessageTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		/// <summary>
		/// Returns all of the <see cref="TASVideos.Data.Entity.Forum.PrivateMessage"/>
		/// records where the given <see cref="user"/> is the recipient
		/// </summary>
		public async Task<ForumInboxModel> GetUserInBox(User user)
		{
			return new ForumInboxModel
			{
				UserId = user.Id,
				UserName = user.UserName,
				Inbox = await _db.ForumPrivateMessages
					.ToUser(user)
					.ThatAreToNotUserDeleted()
					.ThatAreNotToUserSaved()
					.Select(pm => new ForumInboxModel.InboxEntry
					{
						Id = pm.Id,
						Subject = pm.Subject,
						SendDate = pm.CreateTimeStamp,
						FromUser = pm.FromUser.UserName,
						IsRead = pm.ReadOn.HasValue
					})
					.ToListAsync()
			};
		}

		/// <summary>
		/// Returns the <see cref="TASVideos.Data.Entity.Forum.PrivateMessage"/>
		/// record with the given <see cref="id"/> that is addressed to the given <see cref="user"/>
		/// </summary>
		public async Task<ForumPrivateMessageModel> GetPrivateMessageToUser(User user, int id)
		{
			var pm = await _db.ForumPrivateMessages
				.Include(p => p.FromUser)
				.ToUser(user)
				.ThatAreToNotUserDeleted()
				.SingleOrDefaultAsync(p => p.Id == id);

			if (pm == null)
			{
				return null;
			}

			pm.ReadOn = DateTime.UtcNow;
			await _db.SaveChangesAsync();
			_cache.Remove(_messageCountCacheKey + user.Id); // Message count possibly no longer valid

			var model = new ForumPrivateMessageModel
			{
				Id = pm.Id,
				Subject = pm.Subject,
				SentOn = pm.CreateTimeStamp,
				Text = pm.Text,
				FromUserId = pm.FromUserId,
				FromUserName = pm.FromUser.UserName
			};

			return model;
		}

		/// <summary>
		/// Returns the the number of unread <see cref="TASVideos.Data.Entity.Forum.PrivateMessage"/>
		/// for the given <see cref="User" />
		/// </summary>
		public async Task<int> GetUnreadMessageCount(User user)
		{
			var cacheKey = _messageCountCacheKey + user.Id;
			if (_cache.TryGetValue(cacheKey, out int unreadMessageCount))
			{
				return unreadMessageCount;
			}

			unreadMessageCount = await _db.ForumPrivateMessages
				.ThatAreToNotUserDeleted()
				.ToUser(user)
				.CountAsync(pm => pm.ReadOn == null);

			_cache.Set(cacheKey, unreadMessageCount, DurationConstants.OneMinuteInSeconds);
			return unreadMessageCount;
		}

		// TODO: document
		public async Task SaveMessageToUser(User user, int id)
		{
			var message = await _db.ForumPrivateMessages
				.ToUser(user)
				.ThatAreToNotUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == id);

			if (message != null)
			{
				message.SavedForToUser = true;
				await _db.SaveChangesAsync();
			}
		}

		public async Task DeleteMessageToUser(User user, int id)
		{
			var message = await _db.ForumPrivateMessages
				.ToUser(user)
				.ThatAreToNotUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == id);

			if (message != null)
			{
				message.DeletedForToUser = true;
				await _db.SaveChangesAsync();
			}
		}
	}
}
