using System.Collections.Concurrent;
using System.Security.Claims;

namespace TASVideos.Middleware;

public class PermissionMiddleware(RequestDelegate next)
{
	private static readonly ConcurrentDictionary<int, List<PermissionTo>> _userPermissionsCache = new();

	public async Task Invoke(HttpContext context, IUserManager userManager)
	{
		if (context.User.IsLoggedIn())
		{
			var claimsIdentity = context.User.Identity as ClaimsIdentity;
			if (claimsIdentity is not null)
			{
				var userId = context.User.GetUserId();
				if (!_userPermissionsCache.TryGetValue(userId, out var userPermissions))
				{
					userPermissions = (await userManager.GetUserPermissionsById(userId)).ToList();
					_userPermissionsCache[userId] = userPermissions;
				}

				// this should be empty, but just in case, remove existing permissions
				foreach (var claim in claimsIdentity.Claims
							.ThatArePermissions()
							.ToList())
				{
					claimsIdentity.RemoveClaim(claim);
				}

				claimsIdentity.AddClaims(userPermissions.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString())));
			}
		}

		await next(context);
	}

	public static void ClearAllUsersPermissionsCache()
	{
		_userPermissionsCache.Clear();
	}
}
