using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Services
{
	public class UserManager : UserManager<User>
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		// Holy dependencies, batman
		public UserManager(
			ApplicationDbContext db,
			ICacheService cache,
			IUserStore<User> store,
			IOptions<IdentityOptions> optionsAccessor,
			IPasswordHasher<User> passwordHasher,
			IEnumerable<IUserValidator<User>> userValidators,
			IEnumerable<IPasswordValidator<User>> passwordValidators,
			ILookupNormalizer keyNormalizer,
			IdentityErrorDescriber errors,
			IServiceProvider services,
			ILogger<UserManager<User>> logger)
			: base(
				store,
				optionsAccessor,
				passwordHasher,
				userValidators,
				passwordValidators,
				keyNormalizer,
				errors,
				services,
				logger)
		{
			_cache = cache;
			_db = db;
		}

		/// <summary>
		/// Returns the the number of unread <see cref="PrivateMessage"/>
		/// for the given <see cref="User" />
		/// </summary>
		public async Task<int> GetUnreadMessageCount(int userId)
		{
			var cacheKey = CacheKeys.UnreadMessageCount + userId;
			if (_cache.TryGetValue(cacheKey, out int unreadMessageCount))
			{
				return unreadMessageCount;
			}

			unreadMessageCount = await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.ToUser(userId)
				.CountAsync(pm => pm.ReadOn == null);

			_cache.Set(cacheKey, unreadMessageCount, Durations.OneMinuteInSeconds);
			return unreadMessageCount;
		}
	}
}
