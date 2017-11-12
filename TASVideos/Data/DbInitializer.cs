using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TASVideos.Data.Entity;
using TASVideos.Data.SampleData;
using TASVideos.Data.SeedData;

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

			context.Permissions.AddRange(PermissionSeedData.Permissions);
			context.Roles.AddRange(RoleSeedData.Roles);
			context.SaveChanges();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitruary data for testing purposes and would not be apart of a production release
		/// </summary>
		public static async Task GenerateDevSampleData(ApplicationDbContext context, UserManager<User> userManager)
		{
			foreach (var user in UserSampleData.Users)
			{
				var result = await userManager.CreateAsync(user, UserSampleData.SamplePassword);
				if (!result.Succeeded)
				{
					throw new Exception(string.Join(",", result.Errors.Select(e => e.ToString())));
				}


				var savedUser = context.Users.Single(u => u.UserName == user.UserName);
				savedUser.EmailConfirmed = true;
				savedUser.LockoutEnabled = false; // TODO: only for admins

				foreach (var role in context.Roles.ToList()) // TODO: only for admins
				{
					context.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = savedUser.Id });
				}
			}

			await context.SaveChangesAsync();
		}
	}
}
