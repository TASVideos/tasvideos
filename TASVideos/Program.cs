using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Legacy;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Site;
using TASVideos.Services;

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
					var env = services.GetRequiredService<IHostingEnvironment>();
					var context = services.GetRequiredService<ApplicationDbContext>();
					var settings = services.GetRequiredService<IOptions<AppSettings>>().Value;

					if (env.IsDevelopment())
					{
						var userManager = services.GetRequiredService<UserManager>();
						DbInitializer.Initialize(context);
						DbInitializer.PreMigrateSeedData(context);
						DbInitializer.PostMigrateSeedData(context);
						DbInitializer.GenerateDevTestUsers(context, userManager).Wait();
						DbInitializer.GenerateDevSampleData(context, userManager).Wait();
					}
					else if (env.IsLocalWithoutRecreate() || env.IsDemo())
					{
						context.Database.EnsureCreated();
					}
					else if (env.IsLocalWithImport())
					{
						var legacySiteContext = services.GetRequiredService<NesVideosSiteContext>();
						var legacyForumContext = services.GetRequiredService<NesVideosForumContext>();
						var userManager = services.GetRequiredService<UserManager>();

						DbInitializer.Initialize(context);
						DbInitializer.PreMigrateSeedData(context);
						LegacyImporter.RunLegacyImport(context, settings.ConnectionStrings.DefaultConnection, legacySiteContext, legacyForumContext);
						DbInitializer.PostMigrateSeedData(context);
						DbInitializer.GenerateDevTestUsers(context, userManager).Wait();
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

		public static IWebHost BuildWebHost(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddCommandLine(args)
				.Build();

			return WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(config)
				.UseStartup<Startup>()
				.Build();
		}
	}
}
