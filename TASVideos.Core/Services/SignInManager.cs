using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Data;
using TASVideos.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Core.Services
{
	public class SignInManager : SignInManager<User>
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager _userManager;

		public SignInManager(
			ApplicationDbContext db,
			UserManager userManager,
			IHttpContextAccessor contextAccessor,
			IUserClaimsPrincipalFactory<User> claimsFactory,
			IOptions<IdentityOptions> optionsAccessor,
			ILogger<SignInManager<User>> logger,
			IAuthenticationSchemeProvider schemes,
			IUserConfirmation<User> confirmation)
			: base(
				userManager,
				contextAccessor,
				claimsFactory,
				optionsAccessor,
				logger,
				schemes,
				confirmation)
		{
			_db = db;
			_userManager = userManager;
		}

		public async Task<SignInResult> SignInWithLegacySupport(string userName, string password, bool rememberMe = false)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
			if (user == null)
			{
				return SignInResult.Failed;
			}

			// If no password, then try to log in with legacy method
			if (!string.IsNullOrWhiteSpace(user.LegacyPassword))
			{
				using var md5 = MD5.Create();
				var md5Result = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
				string encrypted = BitConverter.ToString(md5Result)
					.Replace("-", "")
					.ToLower();

				if (encrypted == user.LegacyPassword)
				{
					user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
					await _userManager.UpdateSecurityStampAsync(user);
					user.LegacyPassword = null;
					await _db.SaveChangesAsync();
				}
			}

			await _userManager.AddUserPermissionsToClaims(user);
			var result = await base.PasswordSignInAsync(
				userName,
				password,
				rememberMe,
				lockoutOnFailure: true);

			if (result.Succeeded)
			{
				var loggedInUser = await _db.Users.SingleAsync(u => u.UserName == userName);
				loggedInUser.LastLoggedInTimeStamp = DateTime.UtcNow;
				await _db.SaveChangesAsync();
			}

			return result;
		}
	}
}
