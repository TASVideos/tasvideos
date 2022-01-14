using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
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

		public static IApplicationBuilder UseExceptionHandlers(this IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			return app.UseMiddleware(typeof(ErrorHandlingMiddleware));
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
				// By default, cache non-logged in responses for 30 seconds, but do not cache logged in users
				// Any page can override these defaults with the ResponseCache attribute
				// We do not want to cache logged in responses as that can cause usability issues for a user as they log in/out of a cached page
				if (context.User.IsLoggedIn())
				{
					context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
					{
						NoCache = true
					};
				}
				else
				{
					context.Response.GetTypedHeaders().CacheControl =
						new CacheControlHeaderValue
						{
							Public = true,
							MaxAge = TimeSpan.FromSeconds(30)
						};
				}

				// Let the reverse proxy do the work
				if (!env.IsProduction() && !env.IsStaging())
				{
					context.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };
				}

				await next();
			});

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
			IWebHostEnvironment env)
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
