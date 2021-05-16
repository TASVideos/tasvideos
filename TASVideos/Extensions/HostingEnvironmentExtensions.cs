using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TASVideos.Extensions
{
	public static class HostingEnvironmentExtensions
	{
		public static bool IsDemo(this IWebHostEnvironment env)
		{
			return env.IsEnvironment("Demo");
		}
	}
}
