using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Models;
using TASVideos.Services;

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

		/// <summary>
		/// Returns the <see cref="TASVideos.Data.Entity.Forum.PrivateMessage"/>
		/// record with the given <see cref="id"/> if the user has access to the message
		/// </summary>
		public async Task<PrivateMessageModel> GetMessage(int userId, int id)
		{
			var pm = await _db.PrivateMessages
				.Include(p => p.FromUser)
				.Include(p => p.ToUser)
				.Where(p => (!p.DeletedForFromUser && p.FromUserId == userId)
					|| (!p.DeletedForToUser && p.ToUserId == userId))
				.SingleOrDefaultAsync(p => p.Id == id);

			if (pm == null)
			{
				return null;
			}

			// If it is the recipient and the message is not deleted
			if (!pm.ReadOn.HasValue && pm.ToUserId == userId)
			{
				pm.ReadOn = DateTime.UtcNow;
				await _db.SaveChangesAsync();
				_cache.Remove(CacheKeys.UnreadMessageCount + userId); // Message count possibly no longer valid
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
				CanReply = pm.ToUserId == userId,
				EnableBbCode = pm.EnableBbCode,
				EnableHtml = pm.EnableHtml
			};

			return model;
		}
	}
}
