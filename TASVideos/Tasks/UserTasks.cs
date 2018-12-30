using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
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
		public PageOf<UserListModel> GetPageOfUsers(PagedModel paging)
		{
			var data = _db.Users
				.Select(u => new UserListModel
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
		/// Gets all of the <see cref="Role"/>s that the current <see cref="User"/> can assign to another user
		/// The list depends on the current User's <see cref="RolePermission"/> list and also any Roles already assigned to the user
		/// </summary>
		public async Task<IEnumerable<SelectListItem>> GetAllRolesUserCanAssign(int userId, IEnumerable<int> assignedRoles)
		{
			if (assignedRoles == null)
			{
				throw new ArgumentException($"{nameof(assignedRoles)} can not be null");
			}

			var assignedRoleList = assignedRoles.ToList();
			var assignablePermissions = await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Where(rp => rp.CanAssign)
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			return await _db.Roles
				.Where(r => r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
					|| assignedRoleList.Contains(r.Id))
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name,
					Disabled = !r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
						&& assignedRoleList.Any() // EF Core 2.1 issue, needs this or a user with no assigned roles blows up
						&& assignedRoleList.Contains(r.Id)
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
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns the id of the <see cref="User"/>
		/// with the given <see cref="userName"/>
		/// </summary>
		public async Task<int> GetUserIdByName(string userName)
		{
			return await _db.Users
				.Where(u => u.UserName == userName)
				.Select(u => u.Id)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns a <see cref="User"/>  with the given id for the purpose of editing
		/// Which <see cref="Role"/>s are available to assign to the User depends on the User with the given <see cref="currentUserId" />'s <see cref="RolePermission"/> list
		/// </summary>
		public async Task<UserEditModel> GetUserForEdit(string userName, int currentUserId)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var model = await _db.Users
					.ProjectTo<UserEditModel>()
					.SingleAsync(u => u.UserName == userName);

				model.AvailableRoles = await GetAllRolesUserCanAssign(currentUserId, model.SelectedRoles);

				return model;
			}
		}

		/// <summary>
		/// Updates the given <see cref="User"/>
		/// </summary>
		public async Task EditUser(int id, UserEditPostModel model)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			if (model.UserName != user.UserName)
			{
				user.UserName = model.UserName;
			}

			if (model.TimezoneId != user.TimeZoneId)
			{
				user.TimeZoneId = model.TimezoneId;
			}

			user.From = model.From;
			
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

		public async Task UpdateUserProfile(int id, string timezoneId, bool publicRatings, string from)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			user.TimeZoneId = timezoneId;
			user.PublicRatings = publicRatings;
			user.From = from;
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

			_cache.Set(cacheKey, list, Durations.OneMinuteInSeconds);

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
					PublicationActiveCount = u.Publications.Count(p => !p.Publication.ObsoletedById.HasValue),
					PublicationObsoleteCount = u.Publications.Count(p => p.Publication.ObsoletedById.HasValue),
					PublishedSystems = u.Publications.Select(p => p.Publication.System.Code).Distinct(),
					Email = u.Email,
					EmailConfirmed = u.EmailConfirmed,
					Roles = u.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => new RoleBasicDisplay
						{
							Id = ur.RoleId,
							Name = ur.Role.Name,
							Description = ur.Role.Description
						}),
					Submissions = u.Submissions
						.GroupBy(s => s.Submission.Status)
						.Select(g => new UserProfileModel.SubmissionEntry
						{
							Status = g.Key,
							Count = g.Count()
						}),
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
						.Select(p => p.PublicationId)
						.Distinct()
						.CountAsync();
				}
			}

			return model;
		}

		public async Task<IEnumerable<WatchedTopicsModel>> GetUserWatchedTopics(int userId)
		{
			return await _db
				.ForumTopicWatches
				.ForUser(userId)
				.Select(tw => new WatchedTopicsModel
				{
					TopicCreateTimeStamp = tw.ForumTopic.CreateTimeStamp,
					IsNotified = tw.IsNotified,
					ForumId = tw.ForumTopic.ForumId,
					ForumTitle = tw.ForumTopic.Forum.Name,
					TopicId = tw.ForumTopicId,
					TopicTitle = tw.ForumTopic.Title,
				})
				.ToListAsync();
		}

		public async Task StopWatchingTopic(int userId, int topicId)
		{
			try
			{
				var watch = await _db.ForumTopicWatches
					.SingleOrDefaultAsync(tw => tw.UserId == userId && tw.ForumTopicId == topicId);
				_db.ForumTopicWatches.Remove(watch);
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				// Do nothing
				// 1) if a watch is already removed, we are done
				// 2) if a watch was updated (for instance, someone posted in the topic),
				//		there isn't much we can do other than reload the page anyway with an error
				//		An error would only be modestly helpful anyway, and wouldn't save clicks
				//		However, this would be an nice to have one day
			}
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
