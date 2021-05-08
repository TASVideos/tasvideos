using System;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Api;
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
				.AddExternalMediaPublishing(Environment)
				.AddAutoMapperWithProjections()
				.AddSwagger(Settings);

			// Internal Libraries
			services
				.AddTasvideosApi(Settings.Jwt)
				.AddTasvideosData(Configuration, Settings.UsePostgres)
				.AddTasVideosLegacy(
					Settings.ConnectionStrings.LegacySiteConnection,
					Settings.ConnectionStrings.LegacyForumConnection,
					Settings.StartupStrategy() == DbInitializer.StartupStrategy.Import)
				.AddMovieParser();

			// 3rd Party
			services
				.AddMvcWithOptions(Environment)
				.AddIdentity(Environment)
				.AddReCaptcha(Configuration.GetSection("ReCaptcha"));

			// HTTP Client
			services
				.AddHttpClient("Discord", client =>
				{
					client.BaseAddress = new Uri("https://discord.com/api/v6/");
				});
			services
				.AddHttpClient("Twitter", client =>
				{
					client.BaseAddress = new Uri("https://api.twitter.com/1.1/");
				});

			services.AddWebOptimizer(pipeline =>
			{
				pipeline.CompileScssFiles();
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
				.UseWebOptimizer()
				.UseStaticFilesWithTorrents()
				.UseAuthorization()
				.UseAuthentication()
				.UseSwaggerUi(Environment)
				.UseMvcWithOptions();
		}
	}
}
