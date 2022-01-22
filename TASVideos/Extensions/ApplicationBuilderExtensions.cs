using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Serilog;
using TASVideos.Core.Settings;
using TASVideos.Middleware;

namespace TASVideos.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseRobots(this IApplicationBuilder app)
		{
			return app.UseWhen(
				context => context.Request.IsRobotsTxt(),
				appBuilder =>
				{
					appBuilder.UseMiddleware<RobotHandlingMiddleware>();
				});
		}

		public static IApplicationBuilder UseExceptionHandlers(this IApplicationBuilder app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			// TODO: we want to use some middle ware so we can dynamically decide to return json for the API
			// However, registering this in combination with the pages above causes a request to happen a second time
			// when there is is an unhandled exception, which is very bad
			return app;
				////.UseMiddleware(typeof(ErrorHandlingMiddleware));
		}

		public static IApplicationBuilder UseGzipCompression(this IApplicationBuilder app, AppSettings settings)
		{
			if (settings.EnableGzipCompression)
			{
				app.UseResponseCompression();
			}

			return app;
		}

		public static IApplicationBuilder UseStaticFilesWithExtensionMapping(this IApplicationBuilder app)
		{
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".avif"] = "image/avif";
			return app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
		}

		public static IApplicationBuilder UseMvcWithOptions(this IApplicationBuilder app, IHostEnvironment env)
		{
			app.Use(async (context, next) =>
			{
				context.Response.Headers["X-Xss-Protection"] = "1; mode=block";
				context.Response.Headers["X-Frame-Options"] = "DENY";
				context.Response.Headers["X-Content-Type-Options"] = "nosniff";
				context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
				context.Response.Headers["x-powered-by"] = "";
				context.Response.Headers["Content-Security-Policy"] = "upgrade-insecure-requests";
				await next();
			});

			app.UseCookiePolicy(new CookiePolicyOptions
			{
				Secure = CookieSecurePolicy.Always
			});

			app.UseRouting();
			app.UseAuthorization();

			if (!env.IsProduction() && !env.IsStaging())
			{
				app.UseHsts();
			}

			return app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
			});
		}

		public static IApplicationBuilder UseSwaggerUi(
			this IApplicationBuilder app,
			IHostEnvironment env)
		{
			// Append environment to app name when in non-production environments
			var appName = "TASVideos";
			if (!env.IsProduction())
			{
				appName += $" ({env.EnvironmentName})";
			}

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
			return app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", appName);
				c.RoutePrefix = "api";
			});
		}

		public static IApplicationBuilder UseLogging(this IApplicationBuilder app)
		{
			return app.UseSerilogRequestLogging();
		}
	}
}
