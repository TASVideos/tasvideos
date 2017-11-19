using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
    public class RoleSeedData
	{
		private static readonly PermissionTo[] EditorRoles =
		{
			PermissionTo.EditWikiPages,
			PermissionTo.EditGameResources
		};

		private static readonly PermissionTo[] SeniorEditorRoles = EditorRoles.Concat(new[]
		{
			PermissionTo.EditSystemPages
		}).ToArray();

		public static Role AdminRole = new Role
		{
			Name = "Site Admin",
			Description = "This is a site administrator that is responsible for maintaining TASVideos",
			RolePermission = PermissionSeedData.Permissions
				.Select(p => new RolePermission
				{
					Role = AdminRole,
					Permission = p,
					CanAssign = true
				})
				.ToArray()
		};

		public static Role[] Roles =
		{
			new Role
			{
				Name = "Editor",
				Description = "This is a wiki editor that can edit basic wiki pages",
				RolePermission = EditorRoles.Select(p => new RolePermission
				{
					RoleId = 2,
					PermissionId = p
				}).ToArray()
			},
			new Role
			{
				Name = "Senior Editor",
				Description = "This is a wiki editor that can edit any wiki page, including system pages",
				RolePermission = SeniorEditorRoles.Select(p => new RolePermission
				{
					RoleId = 3, // Meh, for lack of a better way
					PermissionId = p
				}).ToArray()
			}
		};
	}
}
