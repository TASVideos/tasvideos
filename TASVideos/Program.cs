using Microsoft.AspNetCore;
using Serilog;
using TASVideos;
using TASVideos.Core.Data;

// Do not remove this class and use top-level design without ensuring EF migrations can still run

var builder = WebApplication.CreateBuilder(args);

/*
AppSettings Settings => Configuration.Get<AppSettings>()!

// Mvc Project Services
services
	.AddAppSettings(Configuration)
	.AddRequestLocalization()
	.AddCookieConfiguration()
	.AddGzipCompression(Settings)
	.AddSwagger(Settings)
	.AddTextModules();

// Internal Libraries
string dbConnection = Settings.UseSampleDatabase
	? Settings.ConnectionStrings.PostgresSampleDataConnection
	: Settings.ConnectionStrings.PostgresConnection;

services
	.AddTasvideosData(Environment.IsDevelopment(), dbConnection)
	.AddTasvideosCore<WikiToTextRenderer>(Environment.IsDevelopment(), Settings)
	.AddMovieParser();

// 3rd Party
services
	.AddMvcWithOptions(Environment)
	.AddIdentity(Environment)
	.AddReCaptcha(Configuration.GetSection("ReCaptcha"));

services.AddWebOptimizer(pipeline =>
{
	pipeline.AddScssBundle("/css/bootstrap.css", "/css/bootstrap.scss");
	pipeline.AddScssBundle("/css/site.css", "/css/site.scss");
	pipeline.AddScssBundle("/css/forum.css", "/css/forum.scss");
});
*/

builder.WebHost.UseSerilog();

// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.Configuration.AddUserSecrets<T>() will not work.
// Manually specify the secret id (matching the csproj) here as a workaround.
builder.Configuration.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");

var app = builder.Build();

/*
app
	.UseRobots()
	.UseMiddleware<Middleware.HtmlRedirectionMiddleware>()
	.UseRequestLocalization()
	.UseExceptionHandlers(env)
	.UseGzipCompression(Settings)
	.UseWebOptimizer()
	.UseStaticFilesWithExtensionMapping()
	.UseAuthentication()
	.UseMiddleware<CustomLocalizationMiddleware>()
	.UseSwaggerUi(Environment)
	.UseLogging()
	.UseMvcWithOptions(env);

if (env.IsDevelopment())
{
	app.UseHttpsRedirection();
}
*/

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
