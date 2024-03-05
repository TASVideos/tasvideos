using Microsoft.AspNetCore;
using Serilog;
using TASVideos;
using TASVideos.Core.Data;

// Do not remove this class and use top-level design without ensuring EF migrations can still run

var builder = WebApplication.CreateBuilder(args);

//.UseStartup<Startup>()

builder.WebHost.UseSerilog();

// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.Configuration.AddUserSecrets<T>() will not work.
// Manually specify the secret id (matching the csproj) here as a workaround.
builder.Configuration.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json")
		.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
		.Build();
	Log.Logger = new LoggerConfiguration()
		.ReadFrom.Configuration(configuration)
		.CreateLogger();

	var services = scope.ServiceProvider;

	await DbInitializer.InitializeDatabase(services);
}

await app.RunAsync();
