using System.Collections.Concurrent;

namespace TASVideos.Core.Services;

public interface IPermissionCacheService
{
	Task<IReadOnlyCollection<PermissionTo>> GetUserPermissions(int userId);
	void ClearUserPermissionsCache(int userId);
	void ClearAllUsersPermissionsCache();
	Task<IReadOnlyCollection<PermissionTo>> GetLiveUserPermissions(int userId, bool getRawPermissions = false);
}

public class PermissionCacheService(ApplicationDbContext db) : IPermissionCacheService
{
	private static readonly ConcurrentDictionary<int, List<PermissionTo>> _userPermissionsCache = new();

	/// <summary>
	/// Returns a list of permissions of a given user.
	/// Returns a cached value if available, otherwise queries the live permissions and fills the cache.
	/// </summary>
	public async Task<IReadOnlyCollection<PermissionTo>> GetUserPermissions(int userId)
	{
		if (!_userPermissionsCache.TryGetValue(userId, out var userPermissions))
		{
			userPermissions = (await GetLiveUserPermissions(userId)).ToList();
			_userPermissionsCache[userId] = userPermissions;
		}

		return userPermissions;
	}

	public void ClearUserPermissionsCache(int userId)
	{
		_userPermissionsCache.TryRemove(userId, out _);
	}

	public void ClearAllUsersPermissionsCache()
	{
		_userPermissionsCache.Clear();
	}

	/// <summary>
	/// Returns a list of all live (uncached) permissions of the <see cref="User"/> with the given id. <br />
	/// By default, "effective" permissions are returned. I.e. even if a banned user has roles with permissions, we still return none. <br />
	/// Set <paramref name="getRawPermissions"/> to return "raw" permissions from the database, which is useful when modifying permissions.
	/// </summary>
	public async Task<IReadOnlyCollection<PermissionTo>> GetLiveUserPermissions(int userId, bool getRawPermissions = false)
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
