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

	public static IApplicationBuilder UseStaticFilesWithExtensionMapping(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		var contentTypeProvider = new FileExtensionContentTypeProvider();
		var staticFileOptions = new StaticFileOptions
		{
			ContentTypeProvider = contentTypeProvider,
			ServeUnknownFileTypes = true,
			DefaultContentType = "text/plain"
		};

		if (env.IsDevelopment())
		{
			staticFileOptions.FileProvider = new DevFallbackFileProvider(env.WebRootFileProvider);
		}

		return app.UseStaticFiles(staticFileOptions);
	}

	public static IApplicationBuilder UseMvcWithOptions(this IApplicationBuilder app, IHostEnvironment env, AppSettings settings)
	{
		string[] trustedJsHosts = [
			"https://cdn.jsdelivr.net",
			"https://cdnjs.cloudflare.com",
			"https://code.jquery.com",
			"https://embed.nicovideo.jp/watch/",
			"https://www.google.com/recaptcha/",
			"https://www.gstatic.com/recaptcha/",
			"https://www.youtube.com",
		];
		string[] cspDirectives = [
			"base-uri 'none'", // neutralises the `<base/>` footgun
			"default-src 'self'", // fallback for other `*-src` directives
			"font-src 'self' https://cdnjs.cloudflare.com/ajax/libs/font-awesome/", // CSS `font: url();` and `@font-face { src: url(); }` will be blocked unless they're from one of these domains (this also blocks nonstandard fonts installed on the system maybe)
			"form-action 'self'", // domains allowed for `<form action/>` (POST target page)
			"frame-src data: 'self' https://embed.nicovideo.jp/watch/ https://www.google.com/recaptcha/ https://www.youtube.com/embed/ https://archive.org/embed/", // allow these domains in <iframe/>
			"img-src * data:", // allow hotlinking images from any domain in UGC (not great)
			$"script-src 'self' {string.Join(' ', trustedJsHosts)}", // `<script/>`s will be blocked unless they're from one of these domains
			"style-src 'unsafe-inline' 'self' https://cdnjs.cloudflare.com/ajax/libs/font-awesome/", // allow `<style/>`, and `<link rel="stylesheet"/>` if it's from our domain or trusted CDN
			"upgrade-insecure-requests", // browser should automagically replace links to any `http://tasvideos.org/...` URL (in UGC, for example) with HTTPS
		];
		var contentSecurityPolicyValue = string.Join("; ", cspDirectives);
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
			context.Response.Headers.ContentSecurityPolicy = contentSecurityPolicyValue;
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

			if (settings.EnableMetrics)
			{
				endpoints.MapPrometheusScrapingEndpoint().RequireAuthorization(builder =>
				{
					builder.RequireClaim(CustomClaimTypes.Permission, ((int)PermissionTo.SeeDiagnostics).ToString());
				});
			}
		});
	}
}
