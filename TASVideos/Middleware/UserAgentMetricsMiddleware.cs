using TASVideos.Core.Settings;

namespace TASVideos.Middleware;

public class UserAgentMetricsMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context, ITASVideosMetrics metrics)
	{
		metrics.AddUserAgent(context.Request.Headers.UserAgent);

		await next(context);
	}
}

public static class UserAgentMetricsMiddlewareExtensions
{
	public static IApplicationBuilder UseUserAgentMetrics(this IApplicationBuilder app, AppSettings settings)
	{
		if (settings.EnableMetrics)
		{
			return app.UseMiddleware<UserAgentMetricsMiddleware>();
		}

		return app;
	}
}
