using System.Security.Claims;
using TASVideos.Core.Extensions;
using TASVideos.Data.Entity;

namespace TASVideos;

public static class ClaimsPrincipalExtensions
{
	public static bool IsLoggedIn(this ClaimsPrincipal? user)
	{
		return user?.Identity?.IsAuthenticated ?? false;
	}

	public static string Name(this ClaimsPrincipal? user)
	{
		return user?.Identity?.Name ?? "";
	}

	public static int GetUserId(this ClaimsPrincipal? user)
	{
		if (user is null || !user.IsLoggedIn())
		{
			return -1;
		}

		return int.Parse(user.Claims
			.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
	}

	public static IEnumerable<PermissionTo> Permissions(this ClaimsPrincipal? user)
	{
		if (user is null || !user.IsLoggedIn())
		{
			return Enumerable.Empty<PermissionTo>();
		}

		return user.Claims.Permissions();
	}

	public static bool Has(this ClaimsPrincipal user, PermissionTo permission)
	{
		return user.Permissions().Contains(permission);
	}

	public static void ReplacePermissionClaims(this ClaimsPrincipal? user, IEnumerable<Claim> permissions)
	{
		if (user is null || !user.IsLoggedIn())
		{
			return;
		}

		if (user.Identity is not ClaimsIdentity ci)
		{
			return;
		}

		foreach (var claim in user.Claims
			.ThatArePermissions()
			.ToList())
		{
			ci.RemoveClaim(claim);
		}

		ci.AddClaims(permissions);
	}
}
