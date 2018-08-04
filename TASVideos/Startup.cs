using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Legacy.Extensions;
using TASVideos.MovieParsers;

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
				.AddCookieConfiguration(Environment)
				.AddGzipCompression(Settings)
				.AddCacheService(Settings.CacheSettings)
				.AddTasks()
				.AddServices()
				.AddMessengerService(Environment, Settings)
				.AddWikiProvider();

			// Internal Libraries
			services
				.AddTasvideosData(Configuration)
				.AddTasVideosLegacy(Environment.IsLocalWithImport())
				.AddTasvideosMovieParsers();

			// 3rd Party
			services
				.AddMvcWithOptions()
				.AddIdentity()
				.AddAutoMapper();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app
				.UseExceptionHandlers(env)
				.UseGzipCompression(Settings)
				.UseStaticFiles()
				.UseAuthentication()
				.UseMvcWithOptions();
		}
	}
}
