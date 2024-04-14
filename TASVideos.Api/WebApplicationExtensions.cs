using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TASVideos.Api.Controllers;

namespace TASVideos.Api;
public static class WebApplicationExtensions
{
	public static WebApplication UseTasvideosApiEndpoints(this WebApplication app, IHostEnvironment env)
	{
		PublicationsApiMapper.Map(app);
		SubmissionsApiMapper.Map(app);
		UseSwaggerUi(app, env);

		app.UseExceptionHandler(exceptionHandlerApp =>
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

		return app;
	}

	private static IApplicationBuilder UseSwaggerUi(this WebApplication app, IHostEnvironment env)
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
}
