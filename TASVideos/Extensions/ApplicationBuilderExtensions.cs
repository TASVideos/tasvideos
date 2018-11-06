using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
			// Note: out of the box, this middleware will set cache-control
			// public only when user is logged out, else no-cache
			// Which is precisely the behavior we want
			app.UseResponseCaching();
			app.Use(async (context, next) =>
			{
				context.Response.GetTypedHeaders().CacheControl =
					new Microsoft.Net.Http.Headers.CacheControlHeaderValue
					{
						Public = true,
						MaxAge = TimeSpan.FromSeconds(30)
					};
				context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
					new[] { "Accept-Encoding" };

				await next();
			});

			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute("forum-post", "forum/p/{id}", defaults: new { controller = "Forum", action = nameof(ForumController.Post) });
				routes.MapRoute("legacy-post", "forum/viewtopic.php", defaults: new { controller = "Forum", action = nameof(ForumController.LegacyPost) });
				routes.MapRoute("sub-list", "Subs-List", defaults: new { controller = "Submission", action = nameof(SubmissionController.List) });
				routes.MapRoute("players-list", "Players-List", defaults: new { controller = "Publication", action = nameof(PublicationController.Authors) });
				routes.MapRoute("submission", "{id:int}S", defaults: new { controller = "Submission", action = nameof(SubmissionController.View) });
				routes.MapRoute("movie", "{id:int}M", defaults: new { controller = "Publication", action = nameof(PublicationController.View) });
				routes.MapRoute("game", "{id:int}G", defaults: new { controller = "Game", action = nameof(GameController.View) });
				routes.MapRoute("movies", "Movies-{query}", defaults: new { controller = "Publication", action = nameof(PublicationController.List) });
				routes.MapRoute("wiki", "{*url}", defaults: new { controller = "Wiki", action = nameof(WikiController.RenderWikiPage) });
			});

			return app;
		}
	}
}
