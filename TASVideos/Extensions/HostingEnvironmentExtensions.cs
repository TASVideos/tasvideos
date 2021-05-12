using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TASVideos.RazorPages.Extensions
{
	public static class HostingEnvironmentExtensions
	{
		public static bool IsDemo(this IWebHostEnvironment env)
		{
			return env.IsEnvironment("Demo");
		}
	}
}
