using System;

using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TASVideos.Extensions;
using TASVideos.Filter;
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
			if (Environment.IsAnyTestEnvironment())
			{
				services.ConfigureApplicationCookie(options =>
				{
					options.ExpireTimeSpan = TimeSpan.FromDays(90);
				});
			}

			if (Environment.IsLocalWithImport())
			{
				services.AddLegacyContext();
			}

			if (Settings.EnableGzipCompression)
			{
				services.AddGzipCompression();
			}

			services.Configure<AppSettings>(Configuration);

			services
				.AddDbContext(Configuration)
				.AddCacheService(Settings.CacheSettings)
				.AddIdentity()
				.AddWikiProvider()
				.AddMovieParser()
				.AddHttpContext()
				.AddFileService()
				.AddPointsService()
				.AddEmailService()
				.AddTasks();

			services.AddAutoMapper();

			services.AddMvc(options =>
			{
				options.Filters.Add(new SetViewBagAttribute());
			});
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
