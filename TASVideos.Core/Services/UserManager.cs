using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Core.Services;

public interface IUserManager
{
	Task<User?> GetUser(ClaimsPrincipal user);
	Task<User> GetRequiredUser(ClaimsPrincipal user);
	Task<User?> FindById(string? userId);
	Task<User?> FindByEmail(string email);
	Task<UserProfile?> GetUserProfile(string userName, bool includeHiddenUserFiles, bool seeRestrictedPosts);
	string[] GetBannedAvatarSites();
	string? AvatarSiteIsBanned(string? avatar);
	Task<IdentityResult> ChangeEmail(User user, string newEmail, string token);
	Task<bool> Exists(string userName);
	Task<bool> VerifyUserToken(User user, string token);
	Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
	Task MarkEmailConfirmed(User user);
	Task<string> GeneratePasswordResetToken(User user);
	Task AssignAutoAssignableRolesByPost(int userId);
	Task AssignAutoAssignableRolesByPublication(IEnumerable<int> userIds, string publicationTitle);
	Task<bool> CanRenameUser(string oldUserName, string newUserName);
	Task UserNameChanged(User user, string oldName);
	Task<string> GenerateEmailConfirmationToken(User user);
	Task PermaBanUser(int userId);
	void ClearCustomLocaleCache(int userId);
	Task<string> GenerateChangeEmailToken(ClaimsPrincipal claimsUser, string newEmail);
	Task<IReadOnlyCollection<PermissionTo>> GetUserPermissionsById(int userId, bool getRawPermissions = false);
	Task<IEnumerable<Claim>> AddUserPermissionsToClaims(User user);
	bool IsConfirmedEmailRequired();
	Task<IdentityResult> Create(User user, string password);
	Task<IdentityResult> ConfirmEmail(User user, string token);
	Task AddStandardRoles(int userId);
	Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword);
	Task<User?> GetUserByEmailAndUserName(string username, string email);
	Task<bool> IsEmailConfirmed(User user);
	string? NormalizeEmail(string? email);
}

internal class UserManager(
	ApplicationDbContext db,
	ICacheService cache,
	IPointsService pointsService,
	ITASVideoAgent tasVideoAgent,
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
	: UserManager<User>(store,
		optionsAccessor,
		passwordHasher,
		userValidators,
		passwordValidators,
		keyNormalizer,
		errors,
		services,
		logger), IUserManager
{
	public async Task<User?> GetUser(ClaimsPrincipal user) => await GetUserAsync(user);
	public async Task<User> GetRequiredUser(ClaimsPrincipal user) => await GetUserAsync(user)
		?? throw new InvalidOperationException($"Unknown user {user.Identity?.Name}");

	public Task<User?> FindById(string? userId)
		=> FindByIdAsync(userId ?? "");

	public Task<User?> FindByEmail(string email)
		=> FindByEmailAsync(email);

	public Task<IdentityResult> ChangeEmail(User user, string newEmail, string token)
		=> ChangeEmailAsync(user, newEmail, token);

	public Task<bool> VerifyUserToken(User user, string token)
		=> VerifyUserTokenAsync(user, Options.Tokens.PasswordResetTokenProvider, ResetPasswordTokenPurpose, token);

	public Task<string> GeneratePasswordResetToken(User user)
		=> GeneratePasswordResetTokenAsync(user);

	public Task<string> GenerateEmailConfirmationToken(User user)
		=> GenerateEmailConfirmationTokenAsync(user);

	public bool IsConfirmedEmailRequired() => Options.SignIn.RequireConfirmedEmail;
	public Task<IdentityResult> Create(User user, string password)
		=> CreateAsync(user, password);

	public Task<IdentityResult> ConfirmEmail(User user, string token)
		=> ConfirmEmailAsync(user, token);

	public Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword)
		=> ChangePasswordAsync(user, currentPassword, newPassword);

	public Task<bool> IsEmailConfirmed(User user) => IsEmailConfirmedAsync(user);

	/// <summary>
	/// Clears the user claims, and adds a distinct list of user permissions, so they can be stored and retrieved from their cookie
	/// </summary>
	public async Task<IEnumerable<Claim>> AddUserPermissionsToClaims(User user)
	{
		await db.UserClaims
			.Where(u => u.UserId == user.Id)
			.Where(c => c.ClaimType == CustomClaimTypes.Permission)
			.ExecuteDeleteAsync();

		var permissions = await GetUserPermissionsById(user.Id);

		var claims = permissions
			.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString()))
			.ToList();
		await AddClaimsAsync(user, claims);
		return claims;
	}

	/// <summary>
	/// Returns a list of all permissions of the <seea cref="User"/> with the given id. <br />
	/// By default, "effective" permissions are returned. I.e. even if a banned user has roles with permissions, we still return none. <br />
	/// Set <paramref name="getRawPermissions"/> to return "raw" permissions from the database, which is useful when modifying permissions.
	/// </summary>
	public async Task<IReadOnlyCollection<PermissionTo>> GetUserPermissionsById(int userId, bool getRawPermissions = false)
	{
		if (!getRawPermissions)
		{
			// effective permissions
			return await db.Users
				.Where(u => u.Id == userId)
				.ThatAreNotBanned()
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct()
				.ToListAsync();
		}
		else
		{
			// raw permissions
			return await db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct()
				.ToListAsync();
		}
	}

	public async Task AddStandardRoles(int userId)
	{
		var user = await db.Users
			.Include(u => u.UserRoles)
			.SingleAsync(u => u.Id == userId);
		var roles = await db.Roles
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
			db.UserRoles.Add(userRole);
			user.UserRoles.Add(userRole);
		}

		await db.SaveChangesAsync();
	}

	/// <summary>
	/// Returns publicly available user profile information
	/// for the <see cref="User"/> with the given <see cref="userName"/>
	/// If no user is found, null is returned
	/// </summary>
	public async Task<UserProfile?> GetUserProfile(string userName, bool includeHiddenUserFiles, bool seeRestrictedPosts)
	{
		var model = await db.Users
			.ForUser(userName)
			.Select(u => new UserProfile
			{
				Id = u.Id,
				UserName = u.UserName,
				PostCount = u.Posts.Count(p => seeRestrictedPosts || !p.Topic!.Forum!.Restricted),
				JoinedOn = u.CreateTimestamp,
				LastLoggedIn = u.LastLoggedInTimeStamp,
				Avatar = u.Avatar,
				Location = u.From,
				Signature = u.Signature,
				PublicRatings = u.PublicRatings,
				TimeZone = u.TimeZoneId,
				LockedOutStatus = u.LockoutEnabled && u.LockoutEnd.HasValue,
				BannedUntil = u.BannedUntil,
				PublicationActiveCount = u.Publications
					.Count(p => !p.Publication!.ObsoletedById.HasValue),
				PublicationObsoleteCount = u.Publications
					.Count(p => p.Publication!.ObsoletedById.HasValue),
				Email = u.Email,
				EmailConfirmed = u.EmailConfirmed,
				PreferredPronouns = u.PreferredPronouns,
				ModeratorComments = u.ModeratorComments,
				Roles = u.UserRoles
					.Select(ur => new UserProfile.RoleSummary(ur.Role!.Name, ur.Role.Description))
					.ToList(),
				UserFiles = new()
				{
					Total = u.UserFiles.Count(uf => includeHiddenUserFiles || !uf.Hidden)
				}
			})
			.SingleOrDefaultAsync();

		if (model is null)
		{
			return null;
		}

		model.HasHomePage = await wikiPages.Exists(LinkConstants.HomePages + model.UserName);

		model.Submissions = await db.Submissions
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
		var (points, rank) = await pointsService.PlayerPoints(model.Id);
		model.PlayerPoints = (int)Math.Round(points);
		model.PlayerRank = rank;

		model.PublishedSystems = await db.Publications
			.ForAuthor(model.Id)
			.Select(p => p.System!.Code)
			.Distinct()
			.ToListAsync();

		model.UserFiles.Systems = await db.UserFiles
			.Where(uf => uf.AuthorId == model.Id)
			.Where(uf => includeHiddenUserFiles || !uf.Hidden)
			.Select(uf => uf.System!.Code)
			.Distinct()
			.ToListAsync();

		var wikiEdits = await db.WikiPages
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

		model.Publishing = new()
		{
			TotalPublished = await db.Submissions
				.CountAsync(s => s.PublisherId == model.Id)
		};

		model.Judgments = new()
		{
			TotalJudgments = await db.Submissions
				.CountAsync(s => s.JudgeId == model.Id)
		};

		if (model.PublicRatings)
		{
			model.Ratings.TotalMoviesRated = await db.PublicationRatings
				.Where(p => p.Publication!.ObsoletedById == null)
				.Where(p => p.UserId == model.Id)
				.Distinct()
				.CountAsync();
		}

		return model;
	}

	/// <summary>
	/// Assigns any roles to the user that have an auto-assign post count
	/// property, that the user does not already have. Note that the role
	/// won't be assigned if the user already has all permissions assigned to that role
	/// </summary>
	public async Task AssignAutoAssignableRolesByPost(int userId)
	{
		var postCount = await db.ForumPosts.CountAsync(p => p.PosterId == userId);

		if (postCount == 0)
		{
			return;
		}

		var userPermissions = (await GetUserPermissionsById(userId, getRawPermissions: true)).ToList();

		var assignableRoles = await db.Roles
			.Include(r => r.RolePermission)
			.Where(r => r.AutoAssignPostCount <= postCount)
			.ToListAsync();

		foreach (var role in assignableRoles)
		{
			var newRolePermissions = role.RolePermission
				.Select(rp => rp.PermissionId)
				.ToList();

			// If the new role has any permissions that the user does not have,
			// then assign the role. Indirectly this also ensures that
			// the user will not already have the role
			if (newRolePermissions.Any(p => !userPermissions.Contains(p)))
			{
				db.UserRoles.Add(new UserRole
				{
					UserId = userId,
					RoleId = role.Id
				});

				try
				{
					await db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					// Do nothing for now, this can be added manually, in the unlikely situation, that this fails
				}

				await tasVideoAgent.SendAutoAssignedRole(userId, role.Name);
			}
		}
	}

	/// <summary>
	/// Assigns any roles to the user that have auto-assign publication set to true,
	/// that the user does not already have. Note that the role won't be assigned
	/// if the user already has all permissions assigned to that role
	/// </summary>
	public async Task AssignAutoAssignableRolesByPublication(IEnumerable<int> userIds, string publicationTitle)
	{
		var ids = userIds.ToList();

		if (!ids.Any())
		{
			return;
		}

		var assignableRoles = await db.Roles
			.Include(r => r.RolePermission)
			.Where(r => r.AutoAssignPublications)
			.ToListAsync();

		foreach (var userId in ids)
		{
			var hasPublication = await db.PublicationAuthors.AnyAsync(pa => pa.UserId == userId);
			if (!hasPublication)
			{
				continue;
			}

			var userPermissions = (await GetUserPermissionsById(userId, getRawPermissions: true)).ToList();

			foreach (var role in assignableRoles)
			{
				var newRolePermissions = role.RolePermission
					.Select(rp => rp.PermissionId)
					.ToList();

				// If the new role has any permission that the user does not have,
				// then assign the role. Indirectly this also ensures that
				// the user will not already have the role
				if (newRolePermissions.Any(p => !userPermissions.Contains(p)))
				{
					db.UserRoles.Add(new UserRole
					{
						UserId = userId,
						RoleId = role.Id
					});

					await tasVideoAgent.SendPublishedAuthorRole(userId, role.Name, publicationTitle);
				}
			}
		}

		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			// Do nothing for now, this can be added manually, in the unlikely situation, that this fails
		}
	}

	// Hardcoded for now, we can make a database table if this becomes a maintenance burden
	private static readonly string[] BannedAvatarSites = [
		"cdn.discordapp.com",
		"media.discordapp.net",
		"membres.lycos.fr",
		"rphaven.org",
		"usuarios.lycos.es"
	];
	public string[] GetBannedAvatarSites() => BannedAvatarSites;

	public string? AvatarSiteIsBanned(string? avatar)
		=> string.IsNullOrWhiteSpace(avatar)
			? null
			: BannedAvatarSites.FirstOrDefault(avatar.Contains);

	public async Task MarkEmailConfirmed(User user)
	{
		if (!user.EmailConfirmed)
		{
			user.EmailConfirmed = true;
			await db.TrySaveChanges();
		}
	}

	/// <summary>
	/// Performs all the necessary updates after a user's name has been changed
	/// </summary>
	public async Task UserNameChanged(User user, string oldName)
	{
		// Move home page and subpages
		var oldHomePage = LinkConstants.HomePages + oldName;
		var newHomePage = LinkConstants.HomePages + user.UserName;
		await wikiPages.MoveAll(oldHomePage, newHomePage);

		// Update submission titles
		var subsToUpdate = await db.Submissions
			.IncludeTitleTables()
			.Where(s => s.SubmissionAuthors.Any(sa => sa.UserId == user.Id))
			.ToListAsync();
		foreach (var sub in subsToUpdate)
		{
			sub.GenerateTitle();
		}

		// Update publication titles
		var pubsToUpdate = await db.Publications
			.IncludeTitleTables()
			.Where(p => p.Authors.Any(pa => pa.UserId == user.Id))
			.ToListAsync();
		foreach (var pub in pubsToUpdate)
		{
			pub.GenerateTitle();
		}

		await db.SaveChangesAsync();
	}

	public Task<bool> Exists(string userName) => db.Users.Exists(userName);

	public void ClearCustomLocaleCache(int userId)
	{
		cache.Remove(CacheKeys.UsersWithCustomLocale);
		cache.Remove(CacheKeys.CustomUserLocalePrefix + userId);
	}

	public async Task<string> GenerateChangeEmailToken(ClaimsPrincipal claimsUser, string newEmail)
	{
		var user = await GetRequiredUser(claimsUser);
		return await GenerateChangeEmailTokenAsync(user, newEmail);
	}

	public async Task PermaBanUser(int userId)
	{
		var user = await db.Users.FindAsync(userId);
		if (user is null)
		{
			return;
		}

		user.BannedUntil = DateTime.UtcNow.AddYears(100);
		await db.TrySaveChanges();
	}

	public async Task<bool> CanRenameUser(string oldUserName, string newUserName)
	{
		if (string.IsNullOrWhiteSpace(newUserName))
		{
			return false;
		}

		var users = await db.Users
			.Select(user => user.UserName)
			.Where(userName => userName == oldUserName || userName == newUserName)
			.ToListAsync();

		return users.Count == 1 && users[0] == oldUserName;
	}

	public async Task<User?> GetUserByEmailAndUserName(string username, string email)
	{
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
		{
			return null;
		}

		return await db.Users.SingleOrDefaultAsync(u => u.Email == email && u.UserName == username);
	}
}
