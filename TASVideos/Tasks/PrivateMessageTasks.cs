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
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
    public class PrivateMessageTasks
    {
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public PrivateMessageTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		// TODO: document
		public async Task<ForumInboxModel> GetUserInBox(User user)
		{
			return new ForumInboxModel
			{
				UserId = user.Id,
				UserName = user.UserName,
				Inbox = await _db.ForumPrivateMessages
					.Where(pm => pm.ToUserId == user.Id)
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

		// TODO: document
		public async Task<ForumPrivateMessageModel> GetPrivateMessage(User user, int id)
		{
			var pm = await _db.ForumPrivateMessages
				.Include(p => p.FromUser)
				.Where(p => p.Id == id)
				.Where(p => p.ToUserId == user.Id)
				.SingleOrDefaultAsync();

			if (pm == null)
			{
				return null;
			}

			pm.ReadOn = DateTime.UtcNow;
			await _db.SaveChangesAsync();

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

		// TODO: document
		public async Task<int> GetUnreadMessageCount(User user)
		{
			var cacheKey = $"{nameof(ForumTasks)}-{nameof(GetUnreadMessageCount)}-{user.Id}";
			if (_cache.TryGetValue(cacheKey, out int unreadMessageCount))
			{
				return unreadMessageCount;
			}

			unreadMessageCount = await _db.ForumPrivateMessages
				.Where(pm => pm.ToUserId == user.Id)
				.CountAsync(pm => pm.ReadOn == null);

			_cache.Set(cacheKey, unreadMessageCount, DurationConstants.OneMinuteInSeconds);
			return unreadMessageCount;
		}
	}
}
