using System.Text;

namespace TASVideos.Middleware;

#pragma warning disable CS9113 // Parameter is unread.
public class RobotHandlingMiddleware(RequestDelegate request, IHostEnvironment env)
{
	public async Task Invoke(HttpContext context)
	{
		var sb = new StringBuilder();

		if (env.IsProduction())
		{
			// TODO the format for this was recently codified to basically match what Google was using, anyway each entry has an implicit trailing wildcard so there's a small chance of false positives
			sb.AppendLine("""
						User-agent: *
						Disallow: /Movies-
						Disallow: /MovieMaintenanceLog
						Disallow: /UserMaintenanceLog
						Disallow: /Account/
						Disallow: /Forum/Posts/User/
						Disallow: /Forum/Topics/Create/
						Disallow: /InternalSystem/
						Disallow: /Search
						Disallow: /*?revision=*
						Disallow: /Wiki/PageHistory
						Disallow: /Wiki/PageNotFound
						Disallow: /Wiki/Referrers
						Disallow: /Wiki/ViewSource

						User-agent: Fasterfox
						Disallow: /
						""");
		}
		else
		{
			sb
				.AppendLine("User-agent: *")
				.AppendLine("Disallow: / ");
		}

		context.Response.StatusCode = 200;
		context.Response.ContentType = "text/plain";
		await context.Response.WriteAsync(sb.ToString());
	}
}
