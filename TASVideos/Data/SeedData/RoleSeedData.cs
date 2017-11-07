using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
    public class RoleSeedData
	{
		private static readonly PermissionTo[] _editorRoles =
		{
			PermissionTo.EditWikiPages
		};

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
			},
			new Role
			{
				Name = "Editor",
				RolePermission = _editorRoles.Select(p => new RolePermission
				{
					RoleId = 2, // Meh, for lack of a better way
					PermissionId = p
				}).ToArray()
			}
		};
	}
}
