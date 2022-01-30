using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public class UserManager : UserManager<User>
{
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;
	private readonly IPointsService _pointsService;
	private readonly IWikiPages _wikiPages;

	// Holy dependencies, batman
	public UserManager(
		ApplicationDbContext db,
		ICacheService cache,
		IPointsService pointsService,
		IWikiPages wikiPages,
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
		_pointsService = pointsService;
		_wikiPages = wikiPages;
	}

	// Clears the user claims, and adds a distinct list of user permissions
	// so they can be stored and retrieved from their cookie
	public async Task<IEnumerable<Claim>> AddUserPermissionsToClaims(User user)
	{
		var existingClaims = _db.UserClaims
			.Where(u => u.UserId == user.Id)
			.Where(c => c.ClaimType == CustomClaimTypes.Permission);
		_db.UserClaims.RemoveRange(existingClaims);

		var permissions = await GetUserPermissionsById(user.Id);

		var claims = permissions
			.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString()))
			.ToList();
		await AddClaimsAsync(user, claims);
		return claims;
	}

	/// <summary>
	/// Returns a list of all permissions of the <seea cref="User"/> with the given id
	/// </summary>
	public async Task<IEnumerable<PermissionTo>> GetUserPermissionsById(int userId)
	{
		return await _db.Users
			.Where(u => u.Id == userId)
			.SelectMany(u => u.UserRoles)
			.SelectMany(ur => ur.Role!.RolePermission)
			.Select(rp => rp.PermissionId)
			.Distinct()
			.ToListAsync();
	}

	/// <summary>
	/// Returns the the number of unread <see cref="PrivateMessage"/>
	/// for the given <see cref="User" />
	/// </summary>
	public async ValueTask<int> GetUnreadMessageCount(int userId)
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
	public async Task<UserRatings?> GetUserRatings(string userName, bool includeHidden = false)
	{
		var model = await _db.Users
			.Where(u => u.UserName == userName)
			.Select(u => new UserRatings
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
					gkey.Publication!.Title,
					gkey.Publication.ObsoletedById
				},
				gvalue => new
				{
					gvalue.Type,
					gvalue.Value
				})
			.Select(pr => new UserRatings.Rating
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

	public async Task AddStandardRoles(int userId)
	{
		var user = await _db.Users
			.Include(u => u.UserRoles)
			.SingleAsync(u => u.Id == userId);
		var roles = await _db.Roles
			.ThatAreDefault()
			.ToListAsync();

		foreach (var role in roles)
		{
			// Check if user already has role
			if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
			{
				continue;
			}

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
	/// Returns publicly available user profile information
	/// for the <see cref="User"/> with the given <see cref="userName"/>
	/// If no user is found, null is returned
	/// </summary>
	public async Task<UserProfile?> GetUserProfile(string userName, bool includeHiddenUserFiles, bool seeRestrictedPosts)
	{
		var model = await _db.Users
			.Select(u => new UserProfile
			{
				Id = u.Id,
				UserName = u.UserName,
				PostCount = u.Posts.Count(p => seeRestrictedPosts || !p.Topic!.Forum!.Restricted),
				JoinDate = u.CreateTimestamp,
				LastLoggedInTimeStamp = u.LastLoggedInTimeStamp,
				Avatar = u.Avatar,
				Location = u.From,
				Signature = u.Signature,
				PublicRatings = u.PublicRatings,
				TimeZone = u.TimeZoneId,
				IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue,
				PublicationActiveCount = u.Publications
					.Count(p => !p.Publication!.ObsoletedById.HasValue),
				PublicationObsoleteCount = u.Publications
					.Count(p => p.Publication!.ObsoletedById.HasValue),
				Email = u.Email,
				EmailConfirmed = u.EmailConfirmed,
				PreferredPronouns = u.PreferredPronouns,
				Roles = u.UserRoles
					.Select(ur => new RoleDto
					{
						Id = ur.RoleId,
						Name = ur.Role!.Name,
						Description = ur.Role.Description
					})
					.ToList(),
				UserFiles = new UserProfile.UserFileSummary
				{
					Total = u.UserFiles.Count(uf => includeHiddenUserFiles || !uf.Hidden),
				}
			})
			.SingleOrDefaultAsync(u => u.UserName == userName);

		if (model is not null)
		{
			model.Submissions = await _db.Submissions
				.Where(s => s.SubmissionAuthors.Any(sa => sa.UserId == model.Id)
					|| s.Submitter != null && s.SubmitterId == model.Id)
				.GroupBy(s => s.Status)
				.Select(g => new UserProfile.SubmissionEntry
				{
					Status = g.Key,
					Count = g.Count()
				})
				.ToListAsync();

			// TODO: round to 1 digit?
			model.PlayerPoints = (int)Math.Round(await _pointsService.PlayerPoints(model.Id));

			model.PublishedSystems = await _db.Publications
				.Where(p => p.Authors.Any(a => a.UserId == model.Id))
				.Select(p => p.System!.Code)
				.Distinct()
				.ToListAsync();

			model.UserFiles.Systems = _db.UserFiles
				.Where(uf => uf.AuthorId == model.Id)
				.Where(uf => includeHiddenUserFiles || !uf.Hidden)
				.Select(uf => uf.System!.Code)
				.Distinct()
				.ToList();

			var wikiEdits = await _wikiPages.Query
				.ThatAreNotDeleted()
				.CreatedBy(model.UserName)
				.Select(w => new { w.CreateTimestamp })
				.ToListAsync();

			if (wikiEdits.Any())
			{
				model.WikiEdits.TotalEdits = wikiEdits.Count;
				model.WikiEdits.FirstEdit = wikiEdits.Min(w => w.CreateTimestamp);
				model.WikiEdits.LastEdit = wikiEdits.Max(w => w.CreateTimestamp);
			}

			model.Publishing = new UserProfile.PublishingSummary
			{
				TotalPublished = await _db.Submissions
					.CountAsync(s => s.PublisherId == model.Id)
			};

			model.Judgments = new UserProfile.JudgingSummary
			{
				TotalJudgments = await _db.Submissions
					.CountAsync(s => s.JudgeId == model.Id)
			};

			if (model.PublicRatings)
			{
				model.Ratings.TotalMoviesRated = await _db.PublicationRatings
					.Where(p => p.Publication!.ObsoletedById == null)
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
	public async Task<PrivateMessageDto?> GetMessage(int userId, int id)
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

		return new PrivateMessageDto
		{
			Subject = pm.Subject,
			SentOn = pm.CreateTimestamp,
			Text = pm.Text,
			FromUserId = pm.FromUserId,
			FromUserName = pm.FromUser!.UserName,
			ToUserId = pm.ToUserId,
			ToUserName = pm.ToUser!.UserName,
			CanReply = pm.ToUserId == userId,
			EnableBbCode = pm.EnableBbCode,
			EnableHtml = pm.EnableHtml
		};
	}

	/// <summary>
	/// Assigns any roles to the user that have an auto-assign post count
	/// property, that the user does not already have. Note that the role
	/// won't assigned if the user already has all permissions assigned to that role
	/// </summary>
	public async Task AssignAutoAssignableRolesByPost(int userId)
	{
		var postCount = await _db.ForumPosts.CountAsync(p => p.PosterId == userId);

		if (postCount == 0)
		{
			return;
		}

		var userPermissions = (await GetUserPermissionsById(userId)).ToList();

		var assignableRoles = await _db.Roles
			.Include(r => r.RolePermission)
			.Where(r => r.AutoAssignPostCount <= postCount)
			.ToListAsync();

		foreach (var role in assignableRoles)
		{
			var newRolePermissions = role.RolePermission
				.Select(rp => rp.PermissionId)
				.ToList();

			// If the new role has any permission the user does not have
			// then assign the role. Indirectly this also ensures that
			// the user will not already have the role
			if (newRolePermissions.Any(p => !userPermissions.Contains(p)))
			{
				_db.UserRoles.Add(new UserRole
				{
					UserId = userId,
					RoleId = role.Id
				});

				try
				{
					await _db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					// Do nothing for now, this can be added manually, in the unlikely situation, that this fails
				}
			}
		}
	}

	/// <summary>
	/// Assigns any roles to the user that have auto-assign publication set to true,
	/// that the user does not already have. Note that the role won't assigned
	/// if the user already has all permissions assigned to that role
	/// </summary>
	public async Task AssignAutoAssignableRolesByPublication(IEnumerable<int> userIds)
	{
		var ids = userIds.ToList();

		if (!ids.Any())
		{
			return;
		}

		var assignableRoles = await _db.Roles
			.Include(r => r.RolePermission)
			.Where(r => r.AutoAssignPublications)
			.ToListAsync();

		foreach (var userId in ids)
		{
			var hasPublication = await _db.PublicationAuthors.AnyAsync(pa => pa.UserId == userId);
			if (!hasPublication)
			{
				continue;
			}

			var userPermissions = (await GetUserPermissionsById(userId)).ToList();

			foreach (var role in assignableRoles)
			{
				var newRolePermissions = role.RolePermission
					.Select(rp => rp.PermissionId)
					.ToList();

				// If the new role has any permission the user does not have
				// then assign the role. Indirectly this also ensures that
				// the user will not already have the role
				if (newRolePermissions.Any(p => !userPermissions.Contains(p)))
				{
					_db.UserRoles.Add(new UserRole
					{
						UserId = userId,
						RoleId = role.Id
					});


				}
			}
		}

		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			// Do nothing for now, this can be added manually, in the unlikely situation, that this fails
		}
	}
}
