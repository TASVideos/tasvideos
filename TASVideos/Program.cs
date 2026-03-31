using AspNetCore.ReCaptcha;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;
using Microsoft.Data.Sqlite;
using Serilog;
using TASVideos.Api;
using TASVideos.Core.Data;
using TASVideos.Core.Settings;
using TASVideos.Middleware;
using TASVideos.MovieParsers;
using TASVideos.Pages.Feed;

var builder = WebApplication.CreateBuilder(args);

// Manually specify the secret id (matching the csproj) here.
builder.Configuration.AddUserSecrets("aspnet-TASVideos-02A8A629-2080-412F-A29C-61E23228B152");

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.CreateLogger();

try
{
	var settings = builder.Configuration.Get<AppSettings>()!;

	// Mvc Project Services
	builder.Services
		.AddAppSettings(builder.Configuration)
		.AddRequestLocalization()
		.AddCookieConfiguration()
		.AddGzipCompression(settings)
		.AddServices();

	// Internal Libraries
	var dbConnection = settings.UseSampleDatabase
		? settings.ConnectionStrings.PostgresSampleDataConnection
		: settings.ConnectionStrings.PostgresConnection;

	builder.Services
		.AddTasvideosData(builder.Environment.IsDevelopment(), dbConnection)
		.AddTasvideosCore<WikiToTextRenderer, WikiToMetaDescriptionRenderer>(builder.Environment.IsDevelopment(), settings)
		.AddTasvideosMovieParsers()
		.AddTasvideosApi(settings);

	builder.Services.AddDbContext<FeedDbContext>(options =>
	{
		options.UseSqlite($"DataSource={Path.Combine(builder.Environment.ContentRootPath, "feed.db")}");
		if (builder.Environment.IsDevelopment())
		{
			options.EnableSensitiveDataLogging();
		}
	});

	// 3rd Party
	JsEngineSwitcher.AllowCurrentProperty = false;
	builder.Services
		.AddRazorPages(builder.Environment)
		.AddIdentity(builder.Environment)
		.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"))
		.AddSingleton<IJsEngineSwitcher>(new JsEngineSwitcher([new V8JsEngineFactory()], V8JsEngine.EngineName))
		.AddWebOptimizer(pipeline =>
		{
			pipeline.AddScssBundle("/css/bootstrap.css", "/css/bootstrap.scss");
			pipeline.AddScssBundle("/css/site.css", "/css/site.scss");
			pipeline.AddScssBundle("/css/forum.css", "/css/forum.scss");
			pipeline.AddFiles("text/javascript", "/js/*");
		})
		.AddMetrics(settings)
		.AddSerilog();

	var app = builder.Build();

	using (var scope = app.Services.CreateScope())
	{
		var db = scope.ServiceProvider.GetRequiredService<FeedDbContext>();
		db.Database.EnsureCreated();
	}

	app
		.UseExceptionHandlers(app.Environment)
		.UseTasvideosApiEndpoints(builder.Environment)
		.UseRobots()
		.UseUserAgentMetrics(settings)
		.UseMiddleware<HtmlRedirectionMiddleware>()
		.UseRequestLocalization()
		.UseGzipCompression(settings)
		.UseWebOptimizer()
		.UseStaticFilesWithExtensionMapping(builder.Environment)
		.UseAuthentication()
		.UseMiddleware<CustomLocalizationMiddleware>()
		.UseSerilogRequestLogging()
		.UseMvcWithOptions(app.Environment, settings);

	if (app.Environment.IsDevelopment())
	{
		app.UseHttpsRedirection();
	}

	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;

		await DbInitializer.InitializeDatabase(services);
	}

	await app.RunAsync();
}
catch (Exception ex) when (!builder.Environment.IsDevelopment()) // let exceptions propagate in development for better debugging support from IDEs
{
	Log.Fatal(ex, "Application terminated unexpectedly");
	throw; // rethrow the exception to make the application exit with a non-zero code
}
finally
{
	Log.CloseAndFlush();
}
