﻿using AspNetCore.ReCaptcha;
using TASVideos.Core;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Middleware;
using TASVideos.Services;

namespace TASVideos;

public class Startup
{
	public Startup(IConfiguration configuration, IWebHostEnvironment env)
	{
		Configuration = configuration;
		Environment = env;
	}

	public IConfiguration Configuration { get; }
	public IWebHostEnvironment Environment { get; }

	private AppSettings Settings => Configuration.Get<AppSettings>()!;

	public void ConfigureServices(IServiceCollection services)
	{
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
	}

	public void Configure(IApplicationBuilder app, IHostEnvironment env)
	{
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
	}
}
