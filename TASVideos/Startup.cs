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
			if (env.IsAnyTestEnvironment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			if (Settings.EnableGzipCompression)
			{
				app.UseResponseCompression();
			}

			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute("sub-list", "Subs-List", defaults: new { controller = "Submission", action = "List" });
				routes.MapRoute("players-list", "Players-List", defaults: new { controller = "Publication", action = "Authors"});
				routes.MapRoute("submission", "{id:int}S", defaults: new { controller = "Submission", action = "View" });
				routes.MapRoute("movie", "{id:int}M", defaults: new { controller = "Publication", action = "View" });
				routes.MapRoute("game", "{id:int}G", defaults: new { controller = "Game", action = "View" });
				routes.MapRoute("movies", "Movies-{query}", defaults: new { controller = "Publication", action = "List" });
				routes.MapRoute("wiki", "{*url}", defaults: new { controller = "Wiki", action = "RenderWikiPage" });
			});
		}
	}
}
