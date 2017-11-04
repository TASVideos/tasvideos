using System.Linq;

using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;

namespace TASVideos.Data
{
	public static class DbInitializer
	{
		public static void Initialize(ApplicationDbContext context)
		{
			// For now, always delete then recreate the database
			// When the datbase is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			context.Permissions.AddRange(PermissionSeedData.Permissions);
			context.Roles.AddRange(RoleSeedData.Roles);
			context.SaveChanges();

			// Give all permissions to the Admin role
			var adminRole = RoleSeedData.Roles.Single(r => r.Name == "Site Admin");
			var rolePermissions = PermissionSeedData.Permissions.Select(p => new RolePermission
			{
				RoleId = adminRole.Id,
				PermissionId = p.Id
			});

			context.RolePermission.AddRange(rolePermissions);

			var publications = new []
			{
				new Publication { DummyProperty = "dummy1" },
				new Publication { DummyProperty = "dummy2" },
				new Publication { DummyProperty = "dummy3" },
			};

			foreach (var p in publications)
			{
				context.Publications.Add(p);
			}

			context.SaveChanges();
		}
	}
}
