using System.Security.Claims;

namespace TASVideos;

public static class ClaimsExtensions
{
	public static IReadOnlyCollection<PermissionTo> Permissions(this IEnumerable<Claim> claims)
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
