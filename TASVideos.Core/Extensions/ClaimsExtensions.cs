using System.Security.Claims;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Extensions;

public static class ClaimsExtensions
{
	public static IEnumerable<PermissionTo> Permissions(this IEnumerable<Claim> claims)
	{
		return claims
			.Where(c => c.Type == CustomClaimTypes.Permission)
			.Select(c => Enum.Parse<PermissionTo>(c.Value))
			.ToList();
	}

	public static IEnumerable<Claim> ThatArePermissions(this IEnumerable<Claim> claims)
	{
		return claims.Where(c => c.Type == CustomClaimTypes.Permission);
	}
}
