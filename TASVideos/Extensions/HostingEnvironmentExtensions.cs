using Microsoft.AspNetCore.Hosting;

namespace TASVideos.Extensions
{
	public static class HostingEnvironmentExtensions
	{
		public static bool IsDemo(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Demo");
		}
	}
}
