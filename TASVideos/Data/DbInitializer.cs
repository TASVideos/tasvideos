using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;
using TASVideos.Extensions;

namespace TASVideos.Data
{
	public static class DbInitializer
	{
		/// <summary>
		/// Creates the database and seeds it with necessary seed data
		/// Seed data is necessary data for a production release
		/// </summary>
		public static void Initialize(ApplicationDbContext context)
		{
			// For now, always delete then recreate the database
			// When the datbase is more mature we will move towards the Migrations process
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			context.Roles.Add(RoleSeedData.AdminRole);
			context.Roles.AddRange(RoleSeedData.Roles);

			// Make micro roles with 1 permission, for each permission
			// These are useful for giving people 1-off permissions
			foreach (var permission in Enum.GetValues(typeof(PermissionTo)).Cast<PermissionTo>())
			{
				var role = new Role
				{
					Name = permission.EnumDisplayName(),
					Description = $"A role containing only the {permission.EnumDisplayName()} permission"
				};

				role.RolePermission.Add(new RolePermission { Role = role, PermissionId = permission });

				context.Roles.Add(role);
			}

			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitrary data for testing purposes and would not be apart of a production release
		/// </summary>
		public static async Task GenerateDevSampleData(ApplicationDbContext context, UserManager<User> userManager)
		{
			var roles = context.Roles.Where(r => r.Name != RoleSeedData.AdminRole.Name).ToList();

			foreach (var admin in UserSampleData.AdminUsers)
			{
				var result = await userManager.CreateAsync(admin, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedAdminUser = context.Users.Single(u => u.UserName == admin.UserName);
				savedAdminUser.EmailConfirmed = true;
				savedAdminUser.LockoutEnabled = false;

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.AdminRole, User = savedAdminUser });

				// And one random role for testing multi-role
				context.UserRoles.Add(new UserRole { Role = RoleSeedData.Roles.AtRandom(), User = savedAdminUser });
			}

			foreach (var user in UserSampleData.Users)
			{
				var result = await userManager.CreateAsync(user, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}

				var savedUser = context.Users.Single(u => u.UserName == user.UserName);
				savedUser.EmailConfirmed = true;

				context.UserRoles.Add(new UserRole { Role = RoleSeedData.Roles.AtRandom(), User = savedUser });
			}

			// Create lots of throw away users to test things like paging
			for (int i = 1; i <= 41; i++)
			{
				var dummyUser = new User
				{
					UserName = $"Dummy{i}",
					Email = $"Dummy{i}@example.com",
					EmailConfirmed = SampleGenerator.RandomBool(),
					LockoutEnd = SampleGenerator.RandomBool() && SampleGenerator.RandomBool()
						? DateTime.Now.AddMonths(1)
						: (DateTimeOffset?)null
				};

				await userManager.CreateAsync(dummyUser, UserSampleData.SamplePassword);

				if (SampleGenerator.RandomBool())
				{
					var role = roles.AtRandom();
					context.UserRoles.Add(new UserRole { Role = role, User = dummyUser });
				}
			}

			await context.SaveChangesAsync();
		}
	}
}
