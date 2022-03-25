using Microsoft.AspNetCore.Http.Extensions;

namespace TASVideos.Middleware;

public class HtmlRedirectionMiddleware
{
	private readonly RequestDelegate _next;

	public HtmlRedirectionMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public Task Invoke(HttpContext context)
	{
		var request = context.Request;

		var path = request.Path.Value;
		if (path is null || !path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
		|| string.Equals(path, "/api/index.html", StringComparison.OrdinalIgnoreCase))
		{
			return _next(context);
		}

		var redirectUrl = UriHelper.BuildAbsolute(
			request.Scheme,
			request.Host,
			request.PathBase,
			new PathString(path[..^5]),
			request.QueryString);

		context.Response.StatusCode = 301;
		context.Response.Headers["Location"] = redirectUrl;

		return Task.CompletedTask;
	}
}
