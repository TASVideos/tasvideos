using AspNetCore.ReCaptcha;
using Serilog;
using TASVideos.Core;
using TASVideos.Core.Data;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Middleware;
using TASVideos.Services;

var builder = WebApplication.CreateBuilder(args);

AppSettings settings = builder.Configuration.Get<AppSettings>()!;

// Mvc Project Services
builder.Services
	.AddAppSettings(builder.Configuration)
	.AddRequestLocalization()
	.AddCookieConfiguration()
	.AddGzipCompression(settings)
	.AddSwagger(settings)
	.AddTextModules();

// Internal Libraries
string dbConnection = settings.UseSampleDatabase
	? settings.ConnectionStrings.PostgresSampleDataConnection
	: settings.ConnectionStrings.PostgresConnection;

builder.Services
	.AddTasvideosData(builder.Environment.IsDevelopment(), dbConnection)
	.AddTasvideosCore<WikiToTextRenderer>(builder.Environment.IsDevelopment(), settings)
	.AddMovieParser();

// 3rd Party
builder.Services
	.AddMvcWithOptions(builder.Environment)
	.AddIdentity(builder.Environment)
	.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"))
	.AddWebOptimizer(pipeline =>
	{
		pipeline.AddScssBundle("/css/bootstrap.css", "/css/bootstrap.scss");
		pipeline.AddScssBundle("/css/site.css", "/css/site.scss");
		pipeline.AddScssBundle("/css/forum.css", "/css/forum.scss");
	});

builder.WebHost.UseSerilog();

// We use <GenerateAssemblyInfo>false</GenerateAssemblyInfo> to support GitVersionTask.
// This also suppresses the creation of [assembly: UserSecretsId("...")], so builder.Configuration.AddUserSecrets<T>() will not work.
// Manually specify the secret id (matching the csproj) here as a workaround.
builder.Configuration.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");

var app = builder.Build();

app.UseRobots()
	.UseMiddleware<HtmlRedirectionMiddleware>()
	.UseRequestLocalization()
	.UseExceptionHandlers(app.Environment)
	.UseGzipCompression(settings)
	.UseWebOptimizer()
	.UseStaticFilesWithExtensionMapping()
	.UseAuthentication()
	.UseMiddleware<CustomLocalizationMiddleware>()
	.UseSwaggerUi(app.Environment)
	.UseLogging()
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
