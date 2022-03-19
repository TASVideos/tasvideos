using System.Text;

namespace TASVideos.Middleware;

public class RobotHandlingMiddleware
{
	// ReSharper disable once NotAccessedField.Local
	private readonly IHostEnvironment _env;

	/// <summary>
	/// Initializes a new instance of the <see cref="RobotHandlingMiddleware"/> class.
	/// </summary>
	public RobotHandlingMiddleware(RequestDelegate request, IHostEnvironment env)
	{
		_env = env;
	}

	public async Task Invoke(HttpContext context)
	{
		var sb = new StringBuilder();

		if (_env.IsProduction())
		{
			sb.AppendLine(@"
User-agent: *
Disallow: /forum/
Disallow: /movies/
Disallow: /submissions/
Disallow: /media/
Disallow: /MovieMaintenanceLog
Disallow: /UserMaintenanceLog
Disallow: /InternalSystem/
Disallow: /*?revision=*
Disallow: /Wiki/Diff

User-agent: Mediapartners-Google
Allow: /forum/

User-agent: Fasterfox
Disallow: /
");
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
