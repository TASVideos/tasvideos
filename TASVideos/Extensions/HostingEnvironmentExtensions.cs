using Microsoft.AspNetCore.Hosting;

namespace TASVideos.Extensions
{
    public static class HostingEnvironmentExtensions
    {
		public static bool IsLocalWithoutRecreate(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Development-NoRecreate");
		}

		public static bool IsLocalWithImport(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Development-Import");
		}

		public static bool IsDemo(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Demo");
		}

		public static bool IsAnyLocal(this IHostingEnvironment env)
		{
			return env.IsLocalWithImport()
				|| env.IsLocalWithoutRecreate();
		}

		public static bool IsAnyTestEnvironment(this IHostingEnvironment env)
		{
			return env.IsDevelopment()
				|| env.IsLocalWithoutRecreate()
				|| env.IsLocalWithImport()
				|| env.IsDemo();
		}
    }
}
