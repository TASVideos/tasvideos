using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Swashbuckle.AspNetCore.Swagger;

namespace TASVideos.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseExceptionHandlers(this IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
			}

			if (env.IsDevelopment() || env.IsDemo())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseMiddleware(typeof(ErrorHandlingMiddleware));
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

			// Browsers seem terrible at this, and this behaves terribly
			// Query strings seem to not be taken into account for instance
			app.Use(async (context, next) =>
			{
				////if (!context.User.Identity.IsAuthenticated)
				////{
				////	context.Response.GetTypedHeaders().CacheControl =
				////		new Microsoft.Net.Http.Headers.CacheControlHeaderValue
				////		{
				////			Public = true,
				////			MaxAge = TimeSpan.FromSeconds(30)
				////		};
				////	context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
				////		new[] { "Accept-Encoding" };
				////}

				context.Response.Headers["X-Xss-Protection"] = "1; mode=block";
				context.Response.Headers["X-Frame-Options"] = "DENY";
				context.Response.Headers["X-Content-Type-Options"] = "nosniff";
				context.Response.Headers["Referrer-Policy"] = "origin-when-cross-origin";
				context.Response.Headers["x-powered-by"] = "";

				// TODO: also add in cdn urls, and styles
				// Also consider images, though that is more complicated because of avatars
				////string baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
				////var scriptSrc = $"script-src 'unsafe-inline' {baseUrl} https://cdnjs.cloudflare.com https://www.googletagmanager.com https://www.google-analytics.com";
				////var styleSrc = $"style-src 'unsafe-inline' {baseUrl} https://cdnjs.cloudflare.com https://use.fontawesome.com";
				////context.Response.Headers["Content-Security-Policy"] = $"{scriptSrc}; {styleSrc}";

				await next();
			});

			app.UseMvc();

			return app;
		}

		public static IApplicationBuilder UseSwaggerUi(
			this IApplicationBuilder app,
			IHostingEnvironment env)
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
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", appName);
				c.RoutePrefix = "api";
			});

			return app;
		}
	}
}
