using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace TASVideos.RazorPages.Middleware
{
	public class RobotHandlingMiddleware
	{
		// ReSharper disable once NotAccessedField.Local
		private readonly IWebHostEnvironment _env;

		/// <summary>
		/// Initializes a new instance of the <see cref="RobotHandlingMiddleware"/> class.
		/// </summary>
		public RobotHandlingMiddleware(RequestDelegate request, IWebHostEnvironment env)
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

User-agent: Mediapartners-Google
Allow: /forum/

User-agent: Fasterfox
Disallow: /

Sitemap: http://tasvideos.org/sitemap.xml");
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
}
