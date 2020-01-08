using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace TASVideos
{
	public class RobotHandlingMiddleware
	{
		private readonly RequestDelegate _request;
		private readonly IWebHostEnvironment _env;

		/// <summary>
		/// Initializes a new instance of the <see cref="RobotHandlingMiddleware"/> class.
		/// </summary>
		public RobotHandlingMiddleware(
			RequestDelegate request,
			IWebHostEnvironment env)
		{
			_request = request;
			_env = env;
		}

		public async Task Invoke(HttpContext context)
		{
			var sb = new StringBuilder();

			if (_env.IsProduction())
			{
				// TODO: delete me
				sb
					.AppendLine("User-agent: *")
					.AppendLine("Disallow: / ");

				// TODO: append various things here
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
