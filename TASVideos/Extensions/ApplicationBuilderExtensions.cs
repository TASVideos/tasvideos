using Microsoft.AspNetCore.StaticFiles;
using TASVideos.Core.Settings;
using TASVideos.Middleware;

namespace TASVideos.Extensions;

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

	public static WebApplication UseExceptionHandlers(this WebApplication app, IHostEnvironment env)
	{
		app.UseExceptionHandler("/Error");
		app.UseStatusCodePagesWithReExecute("/Error");
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

	public static IApplicationBuilder UseStaticFilesWithExtensionMapping(this IApplicationBuilder app)
	{
		var provider = new FileExtensionContentTypeProvider();
		provider.Mappings[".avif"] = "image/avif";
		return app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
	}

	public static IApplicationBuilder UseMvcWithOptions(this IApplicationBuilder app, IHostEnvironment env)
	{
		var permissionsPolicyValue = string.Join(", ", [
			"camera=()", // defaults to `self`
			"display-capture=()", // defaults to `self`
			"fullscreen=()", // defaults to `self`
			"geolocation=()", // defaults to `self`
			"microphone=()", // defaults to `self`
			"publickey-credentials-get=()", // defaults to `self`
			"screen-wake-lock=()", // defaults to `self`
			"web-share=()", // defaults to `self`

			// ...and that's all the non-experimental options listed on MDN as of 2024-04
		]);
		app.Use(async (context, next) =>
		{
			context.Response.Headers["Cross-Origin-Embedder-Policy"] = "unsafe-none"; // this is as unsecure as before, but can't use `credentialless`, due to breaking YouTube Embeds, see https://github.com/TASVideos/tasvideos/issues/1852
			context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
			context.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin"; // TODO this is as unsecure as before; should be `same-site` or `same-origin` when serving auth-gated responses
			context.Response.Headers["Permissions-Policy"] = permissionsPolicyValue;
			context.Response.Headers.XXSSProtection = "1; mode=block";
			context.Response.Headers.XFrameOptions = "DENY";
			context.Response.Headers.XContentTypeOptions = "nosniff";
			context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
			context.Response.Headers.XPoweredBy = "";
			context.Response.Headers.ContentSecurityPolicy = "upgrade-insecure-requests; script-src 'self' https://code.jquery.com https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://www.youtube.com/iframe_api https://www.youtube.com/s/player";
			await next();
		});

		app.UseCookiePolicy(new CookiePolicyOptions
		{
			Secure = CookieSecurePolicy.Always
		});

		app
			.UseRouting()
			.UseAuthorization();

		if (!env.IsProduction() && !env.IsStaging())
		{
			app.UseHsts();
		}

		return app.UseEndpoints(endpoints =>
		{
			endpoints.MapRazorPages();
		});
	}
}
