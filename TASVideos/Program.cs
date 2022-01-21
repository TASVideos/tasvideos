using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TASVideos.Core.Data;

namespace TASVideos
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = BuildWebHost(args);

			using (var scope = host.Services.CreateScope())
			{
				var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
				var configuration = new ConfigurationBuilder()
					.AddJsonFile("appsettings.json")
					.AddJsonFile($"appsettings.{env.EnvironmentName ?? "Development"}.json", true)
					.Build();
				Log.Logger = new LoggerConfiguration()
					.ReadFrom.Configuration(configuration)
					.CreateLogger();

				var services = scope.ServiceProvider;

				DbInitializer.InitializeDatabase(services);
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
				.UseSerilog()
				.ConfigureAppConfiguration((hostContext, builder) =>
				{
					// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
					// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.AddUserSecrets<T>() will not work.
					// Manually specify the secret id (matching the csproj) here as a workaround.
					builder.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");
				})
				.Build();
		}
	}
}
