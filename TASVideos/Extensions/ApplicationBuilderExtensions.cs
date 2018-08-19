using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TASVideos.Controllers;

namespace TASVideos.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseExceptionHandlers(this IApplicationBuilder app, IHostingEnvironment env)
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

			return app;
		}

		public static IApplicationBuilder UseGzipCompression(this IApplicationBuilder app, AppSettings settings)
		{
			if (settings.EnableGzipCompression)
			{
				app.UseResponseCompression();
			}

			return app;
		}

		public static IApplicationBuilder UseMvcWithOptions(this IApplicationBuilder app)
		{
			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute("forum-post", "forum/p/{id}", defaults: new { controller = "Forum", action = nameof(ForumController.Post) });
				routes.MapRoute("sub-list", "Subs-List", defaults: new { controller = "Submission", action = "List" });
				routes.MapRoute("players-list", "Players-List", defaults: new { controller = "Publication", action = "Authors" });
				routes.MapRoute("submission", "{id:int}S", defaults: new { controller = "Submission", action = "View" });
				routes.MapRoute("movie", "{id:int}M", defaults: new { controller = "Publication", action = "View" });
				routes.MapRoute("game", "{id:int}G", defaults: new { controller = "Game", action = "View" });
				routes.MapRoute("movies", "Movies-{query}", defaults: new { controller = "Publication", action = "List" });
				routes.MapRoute("wiki", "{*url}", defaults: new { controller = "Wiki", action = "RenderWikiPage" });
			});

			return app;
		}
	}
}
