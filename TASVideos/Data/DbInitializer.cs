using System.Linq;
using TASVideos.Data.Entity;

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

			if (context.Publications.Any())
			{
				return;   // DB has been seeded
			}

			var permissions = new[]
			{
				new Permision {Id = PermissionTo.EditRoles, Name = "Edit Roles"},
				new Permision {Id = PermissionTo.EditUsers, Name = "Edit Users"}
			};

			context.Permissions.AddRange(permissions);
			context.SaveChanges();

			var adminRole = new Role();
			context.Roles.Add(adminRole);
			context.SaveChanges();


			var rolePermissions = new[]
			{
				new RolePermission
				{
					PermissionId = permissions[0].Id,
					RoleId = adminRole.Id
				},
				new RolePermission
				{
					PermissionId = permissions[1].Id,
					RoleId = adminRole.Id
				}
			};

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
