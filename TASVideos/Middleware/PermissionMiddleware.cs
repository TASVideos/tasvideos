using System.Collections.Concurrent;
using System.Security.Claims;

namespace TASVideos.Middleware;

public class PermissionMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context, IPermissionCacheService permissionCache)
	{
		if (context.User.IsLoggedIn())
		{
			var claimsIdentity = context.User.Identity as ClaimsIdentity;
			if (claimsIdentity is not null)
			{
				var userId = context.User.GetUserId();
				var userPermissions = await permissionCache.GetUserPermissions(userId);

				// this should usually be empty, but remove existing permissions anyway, in case old valid tokens still contain claims
				foreach (var claim in claimsIdentity.Claims.ThatArePermissions().ToList())
				{
					claimsIdentity.RemoveClaim(claim);
				}

				claimsIdentity.AddClaims(userPermissions.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString())));
			}
		}

		await next(context);
	}
}
