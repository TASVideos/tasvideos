using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
    public class RoleSeedData
	{
		public static Role[] Roles =
		{
			new Role
			{
				Name = "Site Admin",
				RolePermission = PermissionSeedData.Permissions.Select(p => new RolePermission
				{
					RoleId = 1, // Meh, for lack of a better way
					PermissionId = p.Id
				}).ToArray()
			}
		};
	}
}
