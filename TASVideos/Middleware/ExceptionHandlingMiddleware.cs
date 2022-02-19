using System.Net;
using System.Text.Json;

namespace TASVideos.Middleware;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;

	public ErrorHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task Invoke(HttpContext context, IHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex, env, logger, _next);
		}
	}

	// https://stackoverflow.com/questions/38630076/asp-net-core-web-api-exception-handling
	private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env, ILogger logger, RequestDelegate next)
	{
		if (context.Request.Path.ToString().Contains("/api/"))
		{
			string result;
			if (env.IsDevelopment())
			{
				result = JsonSerializer.Serialize(new
				{
					Error = exception.Message,
					exception.StackTrace,
					Name = "Unhandled exception: " + exception.GetType().Name,
					exception.InnerException
				});
			}
			else
			{
				result = JsonSerializer.Serialize(new
				{
					Errors = "An error occurred."
				});
			}

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			return context.Response.WriteAsync(result);
		}

		logger.LogError(exception, "An unhandled exception occurred.");
		return next(context);
	}
}
