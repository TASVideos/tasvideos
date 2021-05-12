using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Data;
using TASVideos.RazorPages.Extensions;

namespace TASVideos.RazorPages
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = BuildWebHost(args);

			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;

				try
				{
					DbInitializer.InitializeDatabase(services);
				}
				catch (Exception ex)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "An error occurred while seeding the database.");
				}
			}

			host.Run();
		}

		public static IWebHost BuildWebHost(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddCommandLine(args)
				.Build();

			return WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(config)
				.UseStartup<Startup>()
				.ConfigureAppConfiguration((hostContext, builder) =>
				{
					if (hostContext.HostingEnvironment.IsDevelopment() || hostContext.HostingEnvironment.IsDemo())
					{
						// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
						// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.AddUserSecrets<T>() will not work.
						// Manually specify the secret id (matching the csproj) here as a workaround.
						builder.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");
					}
				})
				.Build();
		}
	}
}
