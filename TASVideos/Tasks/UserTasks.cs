using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		private readonly ICacheService _cache;
		private readonly IPointsService _pointsService;

		public UserTasks(
			ApplicationDbContext db,
			ICacheService cache,
			IPointsService pointsService)
		{
			_db = db;
			_cache = cache;
			_pointsService = pointsService;
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public async Task<IEnumerable<PermissionTo>> GetUserPermissionsById(int userId)
		{
			return await GetUserPermissionByIdQuery(userId)
				.ToListAsync();
		}

		/// <summary>
		/// Checks if the given user name already exists in the database
		/// </summary>
		public async Task<bool> CheckUserNameExists(string userName)
		{
			return await _db.Users.Exists(userName);
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

			_cache.Set(cacheKey, list, Durations.OneMinuteInSeconds);

			return list;
		}

		/// <summary>
		/// Adds standard roles to the given user, these are roles all user's should start with
		/// </summary>
		public async Task AddStandardRolesToUser(int userId)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == userId);
			var roles = await _db.Roles
				.ThatAreDefault()
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
					Entertainment = pr.Where(a => a.Type == PublicationRatingType.Entertainment).Select(a => a.Value).Sum(),
					Tech = pr.Where(a => a.Type == PublicationRatingType.TechQuality).Select(a => a.Value).Sum()
				})
				.ToListAsync();

			// TODO: wrap in transaction
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
				model.PlayerPoints = await _pointsService.CalculatePointsForUser(model.Id);

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
