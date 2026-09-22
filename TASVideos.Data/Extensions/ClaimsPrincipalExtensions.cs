using System.Security.Claims;

namespace TASVideos;

public static class ClaimsPrincipalExtensions
{
	extension(ClaimsPrincipal? user)
	{
		public bool IsLoggedIn() => user?.Identity?.IsAuthenticated ?? false;

		public string Name() => user?.Identity?.Name ?? "";

		public int GetUserId()
			=> user is null || !user.IsLoggedIn()
				? -1
				: int.Parse(user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "-1");

		public IReadOnlyCollection<PermissionTo> Permissions()
			=> user is null || !user.IsLoggedIn()
				? []
				: user.Claims.Permissions();

		public bool Has(PermissionTo permission) => user.Permissions().Contains(permission);

		public bool HasAny(IEnumerable<PermissionTo> permissions)
		{
			var userPermissions = user?.Claims.Permissions() ?? [];
			return permissions.Any(permission => userPermissions.Contains(permission));
		}
	}
}
