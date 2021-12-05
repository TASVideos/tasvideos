using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TASVideos.Core;
using TASVideos.Core.Data;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Extensions;

namespace TASVideos.Legacy
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets<Program>()
				.Build();

			var host = Host
				.CreateDefaultBuilder(args)
				.ConfigureServices((_, services) =>
				{
					services.Configure<AppSettings>(configuration);
					ConfigureServices(services, configuration);
				})
				.Build();

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var settings = configuration.Get<AppSettings>();
				var context = services.GetRequiredService<ApplicationDbContext>();
				var userManager = services.GetRequiredService<UserManager>();
				var legacyImporter = services.GetRequiredService<ILegacyImporter>();
				var cache = services.GetRequiredService<ICacheService>();
				var logger = services.GetRequiredService<ILogger<Program>>();

				if (!cache.TryGetValue(ImportSteps.InitializeAndSeed, out bool _))
				{
					DbInitializer.Initialize(context);
					DbInitializer.PreMigrateSeedData(context);
					cache.Set(ImportSteps.InitializeAndSeed, true, Durations.OneDayInSeconds);
				}
				else
				{
					logger.LogInformation($"Skipping import step: {ImportSteps.InitializeAndSeed}");
				}

				legacyImporter.RunLegacyImport();

				if (!cache.TryGetValue(ImportSteps.PostMigrate, out bool _))
				{
					DbInitializer.PostMigrateSeedData(context);
					cache.Set(ImportSteps.PostMigrate, true, Durations.OneDayInSeconds);
				}
				else
				{
					logger.LogInformation($"Skipping import step: {ImportSteps.PostMigrate}");
				}

				if (!cache.TryGetValue(ImportSteps.GenerateDevUsers, out bool _))
				{
					await DbInitializer.GenerateDevTestUsers(context, userManager, settings);
					cache.Set(ImportSteps.GenerateDevUsers, true, Durations.OneDayInSeconds);
				}
				else
				{
					logger.LogInformation($"Skipping import step: {ImportSteps.GenerateDevUsers}");
				}
			}

			await host.RunAsync();
		}

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			var settings = configuration.Get<AppSettings>();
			services.AddSingleton(settings);
			services
				.AddIdentity<User, Role>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddTasvideosData(configuration, usePostgres: true);
			services.AddTasvideosCore<NullTextRenderer>(true, settings);
			services.AddTasVideosLegacy(
				settings.ConnectionStrings.LegacySiteConnection,
				settings.ConnectionStrings.LegacyForumConnection,
				enable: true);
		}

		private class NullTextRenderer : IWikiToTextRenderer
		{
			public Task<string> RenderWikiForYoutube(WikiPage page) => throw new NotImplementedException();
		}
	}
}
