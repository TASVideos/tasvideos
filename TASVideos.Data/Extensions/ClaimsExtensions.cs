using System.Security.Claims;

namespace TASVideos;

public static class ClaimsExtensions
{
	extension(IEnumerable<Claim> claims)
	{
		public IReadOnlyCollection<PermissionTo> Permissions()
			=> [.. claims
				.Where(c => c.Type == CustomClaimTypes.Permission)
				.Select(c => Enum.Parse<PermissionTo>(c.Value))];

		public IEnumerable<Claim> ThatArePermissions()
			=> claims.Where(c => c.Type == CustomClaimTypes.Permission);
	}
}
