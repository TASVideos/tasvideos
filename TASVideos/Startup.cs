using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Legacy.Extensions;

namespace TASVideos
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			Environment = env;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Environment { get; }

		private AppSettings Settings => Configuration.Get<AppSettings>();

		public void ConfigureServices(IServiceCollection services)
		{
			// Mvc Project Services
			services
				.AddAppSettings(Configuration)
				.AddRequestLocalization()
				.AddCookieConfiguration(Environment)
				.AddGzipCompression(Settings)
				.AddCacheService(Settings.CacheSettings)
				.AddServices(Environment)
				.AddExternalMediaPublishing(Environment, Settings)
				.AddAutoMapperWithProjections()
				.AddSwagger();

			// Internal Libraries
			services
				.AddTasvideosData(Configuration)
				.AddTasVideosLegacy(
					Settings.ConnectionStrings.LegacySiteConnection,
					Settings.ConnectionStrings.LegacyForumConnection,
					Settings.StartupStrategy() == DbInitializer.StartupStrategy.Import)
				.AddMovieParser();

			// 3rd Party
			services
				.AddMvcWithOptions()
				.AddIdentity(Environment);

			// HTTP Client
			services
				.AddHttpClient("Discord", client =>
				{
					client.BaseAddress = new System.Uri("https://discord.com/api/v6/");
				});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app
				.UseRobots()
				.UseRequestLocalization()
				.UseExceptionHandlers(env)
				.UseGzipCompression(Settings)
				.UseHttpsRedirection()
				.UseStaticFiles()
				.UseAuthorization()
				.UseAuthentication()
				.UseSwaggerUi(Environment)
				.UseMvcWithOptions();

			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".torrent"] = "application/x-bittorrent";
		}
	}
}
