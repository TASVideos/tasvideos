using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class UserTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly ICacheService _cache;

		public UserTasks(
			ApplicationDbContext db,
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			ICacheService cache)
		{
			_db = db;
			_userManager = userManager;
			_signInManager = signInManager;
			_cache = cache;
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public IEnumerable<PermissionTo> GetUserPermissionsById(int userId)
		{
			return GetUserPermissionByIdQuery(userId)
				.ToList();
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public async Task<IEnumerable<PermissionTo>> GetUserPermissionsByIdAsync(int userId)
		{
			return await GetUserPermissionByIdQuery(userId)
				.ToListAsync();
		}

		/// <summary>
		/// Signs in the user with the given username and password
		/// If the user has a legacy password, this will be checked and
		/// automatically converted to the new system if successful
		/// </summary>
		/// <returns>A SignInResult indicating the result of the call</returns>
		public async Task<SignInResult> PasswordSignIn(LoginViewModel model)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == model.UserName);
			if (user == null)
			{
				return SignInResult.Failed;
			}

			// If no password, then try to log in with legacy method
			if (!string.IsNullOrWhiteSpace(user.LegacyPassword))
			{
				using (var md5 = MD5.Create())
				{
					var md5Result = md5.ComputeHash(Encoding.ASCII.GetBytes(model.Password));
					string crypted = BitConverter.ToString(md5Result)
						.Replace("-", "")
						.ToLower();

					if (crypted == user.LegacyPassword)
					{
						user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
						await _userManager.UpdateSecurityStampAsync(user);
						user.LegacyPassword = null;
						await _db.SaveChangesAsync();
					}
				}
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, set lockoutOnFailure: true
			return await _signInManager.PasswordSignInAsync(
				model.UserName,
				model.Password,
				model.RememberMe,
				lockoutOnFailure: false);
		}

		/// <summary>
		/// Gets a list of <see cref="Role"/>s that the given user currently has
		/// </summary>
		public async Task<IEnumerable<RoleBasicDisplay>> GetUserRoles(int userId)
		{
			return await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.Select(ur => ur.Role)
				.Select(r => new RoleBasicDisplay
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description
				})
				.ToListAsync();
		}

		/// <summary>
		/// Gets a list of <see cref="User"/>s for the purpose of a user list
		/// </summary>
		public PageOf<UserListViewModel> GetPageOfUsers(PagedModel paging)
		{
			var data = _db.Users
				.Select(u => new UserListViewModel
				{
					Id = u.Id,
					UserName = u.UserName,
					CreateTimeStamp = u.CreateTimeStamp,
					Roles = u.UserRoles
						.Select(ur => ur.Role.Name)
				})
				.SortedPageOf(_db, paging);

			return data;
		}

		/// <summary>
		/// Gets a <see cref="User"/> for the purpose of viewing
		/// </summary>
		public async Task<UserDetailsViewModel> GetUserDetails(int id)
		{
			return await _db.Users
				.Select(u => new UserDetailsViewModel
				{
					Id = u.Id,
					CreateTimeStamp = u.CreateTimeStamp,
					LastLoggedInTimeStamp = u.LastLoggedInTimeStamp,
					UserName = u.UserName,
					Email = u.Email,
					EmailConfirmed = u.EmailConfirmed,
					IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue,
					TimezoneId = u.TimeZoneId,
					PublicRatings = u.PublicRatings,
					Roles = u.UserRoles.Select(ur => ur.Role.Name) // TODO: add .ToList() here to avoid n+1 after 2.1 preview1 bug is fixed
				})
				.SingleAsync(u => u.Id == id);
		}

		/// <summary>
		/// Gets all of the <see cref="Role"/>s that the current <see cref="User"/> can assign to another user
		/// The list depends on the current User's <see cref="RolePermission"/> list and also any Roles already assigned to the user
		/// </summary>
		public async Task<IEnumerable<SelectListItem>> GetAllRolesUserCanAssign(int userId, IEnumerable<int> assignedRoles)
		{
			var assignablePermissions = await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Where(rp => rp.CanAssign)
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			return await _db.Roles
				.Where(r => r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
					|| assignedRoles.Contains(r.Id))
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name,
					Disabled = !r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
						&& assignedRoles.Contains(r.Id)
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns the username of the <see cref="User"/>
		/// with the given <see cref="id"/>
		/// </summary>
		public async Task<string> GetUserNameById(int id)
		{
			return await _db.Users
				.Where(u => u.Id == id)
				.Select(u => u.UserName)
				.SingleAsync();
		}

		/// <summary>
		/// Returns a <see cref="User"/>  with the given id for the purpose of editing
		/// Which <see cref="Role"/>s are available to assign to the User depends on the User with the given <see cref="currentUserId" />'s <see cref="RolePermission"/> list
		/// </summary>
		public async Task<UserEditViewModel> GetUserForEdit(string userName, int currentUserId)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var model = await _db.Users
					.ProjectTo<UserEditViewModel>()
					.SingleAsync(u => u.UserName == userName);

				model.SelectedRoles = await _db.UserRoles
					.Where(ur => ur.UserId == model.Id)
					.Select(ur => ur.RoleId)
					.ToListAsync();

				model.AvailableRoles = await GetAllRolesUserCanAssign(currentUserId, model.SelectedRoles);

				return model;
			}
		}

		/// <summary>
		/// Updates the given <see cref="User"/>
		/// </summary>
		public async Task EditUser(UserEditPostViewModel model)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == model.Id);
			if (model.UserName != user.UserName)
			{
				user.UserName = model.UserName;
			}

			if (model.TimezoneId != user.TimeZoneId)
			{
				user.TimeZoneId = model.TimezoneId;
			}
			
			_db.UserRoles.RemoveRange(_db.UserRoles.Where(ur => ur.User == user));
			await _db.SaveChangesAsync();

			_db.UserRoles.AddRange(model.SelectedRoles
				.Select(r => new UserRole
				{
					User = user,
					RoleId = r
				}));

			await _db.SaveChangesAsync();
		}

		public async Task UpdateUserProfile(int id, string timezoneId, bool publicRatings)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			user.TimeZoneId = timezoneId;
			user.PublicRatings = publicRatings;
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Removes the lock out property on a <see cref="User"/>
		/// </summary>
		public async Task UnlockUser(int id)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			user.LockoutEnd = null;
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Checks if the given user name already exists in the database
		/// </summary>
		public async Task<bool> CheckUserNameExists(string userName)
		{
			return await _db.Users
				.AnyAsync(u => u.UserName == userName);
		}

		/// <summary>
		/// Gets a list of <seealso cref="User"/>s that partially match the given part of a username
		/// </summary>
		public async Task<IEnumerable<string>> GetUsersByPartial(string partialUserName)
		{
			var upper = partialUserName.ToUpper();
			var cacheKey = nameof(GetUsersByPartial) + upper;

			if (_cache.TryGetValue(cacheKey, out List<string> list))
			{
				return list;
			}

			list = await _db.Users
				.Where(u => u.NormalizedUserName.Contains(upper))
				.Select(u => u.UserName)
				.ToListAsync();

			_cache.Set(cacheKey, list, DurationConstants.OneMinuteInSeconds);

			return list;
		}

		/// <summary>
		/// Sets the <see cref="User" /> with the given username's last logged in timestamp to UTC Now
		/// </summary>
		public async Task MarkUserLoggedIn(string userName)
		{
			var user = await _db.Users.SingleAsync(u => u.UserName == userName);
			user.LastLoggedInTimeStamp = DateTime.UtcNow;
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Adds standard roles to the given user, these are roles all user's should start with
		/// </summary>
		public async Task AddStandardRolesToUser(int userId)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == userId);
			var roles = await _db.Roles
				.Where(r => r.IsDefault)
				.ToListAsync();

			foreach (var role in roles)
			{
				var userRole = new UserRole
				{
					UserId = user.Id,
					RoleId = role.Id
				};
				_db.UserRoles.Add(userRole);
				user.UserRoles.Add(userRole);
			}

			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Returns a summary of a <see cref="User" /> with the given <see cref="userName" />
		/// If a user can not be found, null is returned
		/// </summary>
		public async Task<UserSummaryModel> GetUserSummary(string userName)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var user = await _db.Users
					.Select(u => new { u.Id, u.UserName })
					.SingleOrDefaultAsync(u => u.UserName == userName);

				if (user != null)
				{
					return new UserSummaryModel
					{
						Id = user.Id,
						UserName = user.UserName,
						EditCount = _db.WikiPages.Count(wp => wp.CreateUserName == userName),
						MovieCount = _db.Publications
							.Count(p => p.Authors
								.Select(sa => sa.Author.UserName)
								.Contains(userName)),
						SubmissionCount = _db.Submissions
							.Count(s => s.SubmissionAuthors
								.Select(sa => sa.Author.UserName)
								.Contains(userName))
					};
				}

				return null;
			}
		}

		/// <summary>
		/// Returns publically available user profile information
		/// for the <see cref="User"/> with the given <see cref="id"/>
		/// If no user is found, null is returned
		/// </summary>
		public async Task<UserProfileModel> GetUserProfile(int id)
		{
			return await _db.Users
				.Select(u => new UserProfileModel
				{
					Id = u.Id,
					UserName = u.UserName,
					PostCount = u.Posts.Count,
					JoinDate = u.CreateTimeStamp,
					Avatar = u.Avatar,
					Location = u.From,
					Signature = u.Signature,
					PublicationCount = u.Publications.Count,
					PublicationObsoleteCount = u.Publications.Count(p => p.Pubmisison.ObsoletedById.HasValue)
				})
				.SingleOrDefaultAsync(u => u.Id == id);
		}

		private IQueryable<PermissionTo> GetUserPermissionByIdQuery(int userId)
		{
			return _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct();
		}
	}
}
