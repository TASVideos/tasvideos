using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TASVideos.Api;
public static class WebApplicationExtensions
{
	public static IApplicationBuilder UseTasvideosApiEndpoints(this WebApplication app, IHostEnvironment env)
	{
		return app.MapPublications()
			.MapSubmissions()
			.MapGames()
			.MapSystems()
			.MapUsers()
			.MapTags()
			.MapClasses()
			.UseSwaggerUi(env)
			.UseExceptionHandler(exceptionHandlerApp =>
			{
				exceptionHandlerApp.Run(async httpContext =>
				{
					if (httpContext.Request.Path.ToString().StartsWith("/api"))
					{
						var pds = httpContext.RequestServices.GetService<IProblemDetailsService>();
						if (pds == null
							|| !await pds.TryWriteAsync(new() { HttpContext = httpContext }))
						{
							// Fallback behavior
							await httpContext.Response.WriteAsync("Fallback: An error occurred.");
						}
					}
				});
			});
	}

	private static IApplicationBuilder UseSwaggerUi(this IApplicationBuilder app, IHostEnvironment env)
	{
		// Append environment to app name when in non-production environments
		var appName = "TASVideos";
		if (!env.IsProduction())
		{
			appName += $" ({env.EnvironmentName})";
		}

		return app
			.UseSwagger()
			.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", appName);
				c.RoutePrefix = "api";
			});
	}
}
