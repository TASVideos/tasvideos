using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

			var publications = new Publication[]
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
