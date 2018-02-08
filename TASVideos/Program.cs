using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;

namespace TASVideos
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = BuildWebHost(args);

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				try
				{
					var context = services.GetRequiredService<ApplicationDbContext>();
					var legacySiteContext = services.GetRequiredService<NesVideosSiteContext>();
					var legacyForumContext = services.GetRequiredService<NesVideosForumContext>();

					var env = services.GetRequiredService<IHostingEnvironment>();
					var userManager = services.GetRequiredService<UserManager<User>>();
					if (env.IsDevelopment())
					{
						DbInitializer.Initialize(context);
						LegacyImporter.RunLegacyImport(context, legacySiteContext, legacyForumContext);
						DbInitializer.GenerateSeedData(context);
						DbInitializer.GenerateDevSampleData(context, userManager).Wait();
					}
					else if (env.IsStaging())
					{
						DbInitializer.Migrate(context);
						LegacyImporter.RunLegacyImport(context, legacySiteContext, legacyForumContext);
						DbInitializer.GenerateSeedData(context);
					}
				}
				catch (Exception ex)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "An error occurred while seeding the database.");
				}
			}

			host.Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.Build();
	}
}
