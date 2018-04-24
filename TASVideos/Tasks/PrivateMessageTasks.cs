using System;
using System.Collections.Generic;
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
		public async Task<IEnumerable<InboxModel>> GetUserInBox(User user)
		{
			return await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.ThatAreNotToUserSaved()
				.Select(pm => new InboxModel
				{
					Id = pm.Id,
					Subject = pm.Subject,
					SendDate = pm.CreateTimeStamp,
					FromUser = pm.FromUser.UserName,
					IsRead = pm.ReadOn.HasValue
				})
				.ToListAsync();
		}

		// TODO: document
		public async Task<IEnumerable<SaveboxModel>> GetUserSaveBox(User user)
		{
			return await _db.PrivateMessages
				.Where(pm => (pm.SavedForFromUser && !pm.DeletedForFromUser && pm.FromUserId == user.Id)
					|| (pm.SavedForToUser && !pm.DeletedForToUser && pm.ToUserId == user.Id))
				.Select(pm => new SaveboxModel
				{
					Id = pm.Id,
					Subject = pm.Subject,
					FromUser = pm.FromUser.UserName,
					ToUser = pm.ToUser.UserName,
					SendDate = pm.CreateTimeStamp
				})
				.ToListAsync();
		}

		public async Task<IEnumerable<SentboxModel>> GetUserSentBox(User user)
		{
			return await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.Where(pm => pm.FromUserId == user.Id)
				.Select(pm => new SentboxModel
				{
					Id = pm.Id,
					Subject = pm.Subject,
					ToUser = pm.ToUser.UserName,
					SendDate = pm.CreateTimeStamp,
					HasBeenRead = pm.ReadOn.HasValue
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns the <see cref="TASVideos.Data.Entity.Forum.PrivateMessage"/>
		/// record with the given <see cref="id"/> if the user has access to the message
		/// </summary>
		public async Task<PrivateMessageModel> GetMessage(User user, int id)
		{
			var pm = await _db.PrivateMessages
				.Include(p => p.FromUser)
				.Include(p => p.ToUser)
				.Where(p => (!p.DeletedForFromUser && p.FromUserId == user.Id)
					|| (!p.DeletedForToUser && p.ToUserId == user.Id))
				.SingleOrDefaultAsync(p => p.Id == id);

			if (pm == null)
			{
				return null;
			}

			// If it is the recpient and the message is not deleted
			if (!pm.ReadOn.HasValue && pm.ToUserId == user.Id)
			{
				pm.ReadOn = DateTime.UtcNow;
				await _db.SaveChangesAsync();
				_cache.Remove(_messageCountCacheKey + user.Id); // Message count possibly no longer valid
			}

			var model = new PrivateMessageModel
			{
				Id = pm.Id,
				Subject = pm.Subject,
				SentOn = pm.CreateTimeStamp,
				Text = pm.Text,
				FromUserId = pm.FromUserId,
				FromUserName = pm.FromUser.UserName,
				ToUserId = pm.ToUserId,
				ToUserName = pm.ToUser.UserName,
				CanReply = pm.ToUserId == user.Id
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

			unreadMessageCount = await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.ToUser(user)
				.CountAsync(pm => pm.ReadOn == null);

			_cache.Set(cacheKey, unreadMessageCount, DurationConstants.OneMinuteInSeconds);
			return unreadMessageCount;
		}

		// TODO: document
		public async Task SaveMessageToUser(User user, int id)
		{
			var message = await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == id);

			if (message != null)
			{
				message.SavedForToUser = true;
				await _db.SaveChangesAsync();
			}
		}

		// TODO: document
		public async Task DeleteMessageToUser(User user, int id)
		{
			var message = await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == id);

			if (message != null)
			{
				message.DeletedForToUser = true;
				await _db.SaveChangesAsync();
			}
		}

		// TODO: document
		public async Task SendMessage(User user, PrivateMessageCreateModel model, string ipAddress)
		{
			var toUserId = await _db.Users
				.Where(u => u.UserName == model.ToUser)
				.Select(u => u.Id)
				.SingleAsync();

			var message = new PrivateMessage
			{
				FromUserId = user.Id,
				ToUserId = toUserId,
				Subject = model.Subject,
				Text = model.Text,
				IpAddress = ipAddress
			};

			_db.PrivateMessages.Add(message);
			await _db.SaveChangesAsync();
		}
	}
}
