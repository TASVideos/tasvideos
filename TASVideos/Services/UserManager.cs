using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public class UserManager : UserManager<User>
	{
		private readonly ApplicationDbContext _db;

		// Holy dependencies, batman
		public UserManager(
			ApplicationDbContext db,
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
			_db = db;
		}
	}
}
