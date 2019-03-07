using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Legacy.Extensions;

namespace TASVideos
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			Configuration = configuration;
			Environment = env;
		}

		public IConfiguration Configuration { get; }
		public IHostingEnvironment Environment { get; }

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
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app
				.UseRequestLocalization()
				.UseExceptionHandlers(env)
				.UseGzipCompression(Settings)
				.UseStaticFiles()
				.UseAuthentication()
				.UseSwaggerUi(Environment)
				.UseMvcWithOptions();
		}
	}
}
