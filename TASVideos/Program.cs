using AspNetCore.ReCaptcha;
using Serilog;
using TASVideos.Api;
using TASVideos.Core.Data;
using TASVideos.Core.Settings;
using TASVideos.Middleware;
using TASVideos.MovieParsers;
using TASVideos.Services;
var builder = WebApplication.CreateBuilder(args);

// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.Configuration.AddUserSecrets<T>() will not work.
// Manually specify the secret id (matching the csproj) here as a workaround.
builder.Configuration.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");

AppSettings settings = builder.Configuration.Get<AppSettings>()!;

// Mvc Project Services
builder.Services
	.AddAppSettings(builder.Configuration)
	.AddRequestLocalization()
	.AddCookieConfiguration()
	.AddGzipCompression(settings)
	.AddTextModules();

// Internal Libraries
string dbConnection = settings.UseSampleDatabase
	? settings.ConnectionStrings.PostgresSampleDataConnection
	: settings.ConnectionStrings.PostgresConnection;

builder.Services
	.AddTasvideosData(builder.Environment.IsDevelopment(), dbConnection)
	.AddTasvideosCore<WikiToTextRenderer>(builder.Environment.IsDevelopment(), settings)
	.AddTasvideosMovieParsers()
	.AddTasvideosApi(settings);

// 3rd Party
builder.Services
	.AddRazorPages(builder.Environment)
	.AddIdentity(builder.Environment)
	.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"))
	.AddWebOptimizer(pipeline =>
	{
		pipeline.AddScssBundle("/css/bootstrap.css", "/css/bootstrap.scss");
		pipeline.AddScssBundle("/css/site.css", "/css/site.scss");
		pipeline.AddScssBundle("/css/forum.css", "/css/forum.scss");
	});

builder.Host.UseSerilog();

var app = builder.Build();

app
	.UseExceptionHandlers(app.Environment)
	.UseTasvideosApiEndpoints(builder.Environment)
	.UseRobots()
	.UseMiddleware<HtmlRedirectionMiddleware>()
	.UseRequestLocalization()
	.UseGzipCompression(settings)
	.UseWebOptimizer()
	.UseStaticFilesWithExtensionMapping()
	.UseAuthentication()
	.UseMiddleware<CustomLocalizationMiddleware>()
	.UseSerilogRequestLogging()
	.UseMvcWithOptions(app.Environment);

if (app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json")
	.AddJsonFile($"appsettings.{app.Environment.EnvironmentName}.json", true)
	.Build();
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(configuration)
	.CreateLogger();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;

	await DbInitializer.InitializeDatabase(services);
}

await app.RunAsync();
