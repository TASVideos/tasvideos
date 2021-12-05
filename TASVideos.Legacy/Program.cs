using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

				DbInitializer.Initialize(context);
				DbInitializer.PreMigrateSeedData(context);
				legacyImporter.RunLegacyImport();
				DbInitializer.PostMigrateSeedData(context);
				await DbInitializer.GenerateDevTestUsers(context, userManager, settings);
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
