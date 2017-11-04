using TASVideos.Data.Entity;
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
		public static void GenerateDevSampleData(ApplicationDbContext context)
		{
			var publications = new[]
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
