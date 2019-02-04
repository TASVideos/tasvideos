using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;

namespace TASVideos.Services
{
	// TODO: consider not using view models as a return type, these methods should return a lower level abstraction, not a presentation layer object
	public class UserManager : UserManager<User>
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;
		private readonly IPointsCalculator _pointsCalculator;

		// Holy dependencies, batman
		public UserManager(
			ApplicationDbContext db,
			ICacheService cache,
			IPointsCalculator pointsCalculator,
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
			_pointsCalculator = pointsCalculator;
		}

		// Adds a distinct list of user permissions to their claims so they can be stored
		// and retrieved from their cookie
		public async Task AddUserPermissionsToClaims(User user)
		{
			var existingClaims = await GetClaimsAsync(user);
			if (existingClaims.Any(c => c.Type == CustomClaimTypes.Permission))
			{
				return;
			}

			var permissions = await GetUserPermissionsById(user.Id);
			await AddClaimsAsync(user, permissions
				.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString())));
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public async Task<IEnumerable<PermissionTo>> GetUserPermissionsById(int userId)
		{
			return await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct()
				.ToListAsync();
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

		/// <summary>
		/// Returns the rating information for the given user
		/// If user is not found, null is returned
		/// If user has PublicRatings false, then the ratings will be an empty list
		/// </summary>
		public async Task<UserRatingsModel> GetUserRatings(string userName, bool includeHidden = false)
		{
			var model = await _db.Users
				.Where(u => u.UserName == userName)
				.Select(u => new UserRatingsModel
				{
					Id = u.Id,
					UserName = u.UserName,
					PublicRatings = u.PublicRatings
				})
				.SingleOrDefaultAsync();

			if (model == null)
			{
				return null;
			}

			if (!model.PublicRatings && !includeHidden)
			{
				return model;
			}

			model.Ratings = await _db.PublicationRatings
				.ForUser(model.Id)
				.GroupBy(
					gkey => new
					{
						gkey.PublicationId,
						gkey.Publication.Title,
						gkey.Publication.ObsoletedById
					}, 
					gvalue => new
					{
						gvalue.Type,
						gvalue.Value
					})
				.Select(pr => new UserRatingsModel.Rating
				{
					PublicationId = pr.Key.PublicationId,
					PublicationTitle = pr.Key.Title,
					IsObsolete = pr.Key.ObsoletedById.HasValue,
					Entertainment = pr
						.Where(a => a.Type == PublicationRatingType.Entertainment)
						.Select(a => a.Value)
						.Sum(),
					Tech = pr
						.Where(a => a.Type == PublicationRatingType.TechQuality)
						.Select(a => a.Value)
						.Sum()
				})
				.ToListAsync();

			return model;
		}

		/// <summary>
		/// Returns publicly available user profile information
		/// for the <see cref="User"/> with the given <see cref="userName"/>
		/// If no user is found, null is returned
		/// </summary>
		public async Task<UserProfileModel> GetUserProfile(string userName, bool includeHidden)
		{
			var model = await _db.Users
				.Select(u => new UserProfileModel
				{
					Id = u.Id,
					UserName = u.UserName,
					PostCount = u.Posts.Count,
					JoinDate = u.CreateTimeStamp,
					LastLoggedInTimeStamp = u.LastLoggedInTimeStamp,
					Avatar = u.Avatar,
					Location = u.From,
					Signature = u.Signature,
					PublicRatings = u.PublicRatings,
					TimeZone = u.TimeZoneId,
					IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue,
					PublicationActiveCount = u.Publications
						.Count(p => !p.Publication.ObsoletedById.HasValue),
					PublicationObsoleteCount = u.Publications
						.Count(p => p.Publication.ObsoletedById.HasValue),
					PublishedSystems = u.Publications
						.Select(p => p.Publication.System.Code)
						.Distinct()
						.ToList(),
					Email = u.Email,
					EmailConfirmed = u.EmailConfirmed,
					Roles = u.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => new RoleBasicDisplay
						{
							Id = ur.RoleId,
							Name = ur.Role.Name,
							Description = ur.Role.Description
						})
						.ToList(),
					Submissions = u.Submissions
						.GroupBy(s => s.Submission.Status)
						.Select(g => new UserProfileModel.SubmissionEntry
						{
							Status = g.Key,
							Count = g.Count()
						})
						.ToList(),
					UserFiles = new UserProfileModel.UserFilesModel
					{
						Total = u.UserFiles.Count(uf => includeHidden || !uf.Hidden),
						Systems = u.UserFiles
							.Where(uf => includeHidden || !uf.Hidden)
							.Select(uf => uf.System.Code)
							.Distinct()
							.ToList()
					}
				})
				.SingleOrDefaultAsync(u => u.UserName == userName);

			if (model != null)
			{
				model.PlayerPoints = await _pointsCalculator.PlayerPoints(model.Id);

				var wikiEdits = await _db.WikiPages
					.ThatAreNotDeleted()
					.CreatedBy(model.UserName)
					.Select(w => new { w.CreateTimeStamp })
					.ToListAsync();

				if (wikiEdits.Any())
				{
					model.WikiEdits.TotalEdits = wikiEdits.Count;
					model.WikiEdits.FirstEdit = wikiEdits.Min(w => w.CreateTimeStamp);
					model.WikiEdits.LastEdit = wikiEdits.Max(w => w.CreateTimeStamp);
				}

				if (model.PublicRatings)
				{
					model.Ratings.TotalMoviesRated = await _db.PublicationRatings
						.Where(p => p.UserId == model.Id)
						.Distinct()
						.CountAsync();
				}
			}

			return model;
		}

		/// <summary>
		/// Returns the <see cref="PrivateMessage"/>
		/// record with the given <see cref="id"/> if the user has access to the message
		/// A user has access if they are the sender or the receiver of the message
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
