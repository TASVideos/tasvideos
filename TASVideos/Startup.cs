using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TASVideos.Core;
using TASVideos.Core.Data;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			Environment = env;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Environment { get; }

		private AppSettings Settings => Configuration.Get<AppSettings>();

		public void ConfigureServices(IServiceCollection services)
		{
			// Mvc Project Services
			services
				.AddAppSettings(Configuration)
				.AddRequestLocalization()
				.AddCookieConfiguration(Environment)
				.AddGzipCompression(Settings)
				.AddAutoMapperWithProjections()
				.AddSwagger(Settings)
				.AddTextModules();

			// Internal Libraries
			string dbConnection = Settings.GetStartupStrategy() == StartupStrategy.Sample
				? Settings.ConnectionStrings.PostgresSampleDataConnection
				: Settings.ConnectionStrings.PostgresConnection;

			services
				.AddTasvideosData(dbConnection)
				.AddTasvideosCore<WikiToTextRenderer>(Environment.IsDevelopment(), Settings)
				.AddMovieParser();

			// 3rd Party
			services
				.AddMvcWithOptions(Environment)
				.AddIdentity(Environment)
				.AddReCaptcha(Configuration.GetSection("ReCaptcha"));

			services.AddWebOptimizer(pipeline =>
			{
				pipeline.AddScssBundle("/css/site.css", "/css/site.scss");
				pipeline.AddScssBundle("/css/darkmode.css", "/css/darkmode.scss");
				pipeline.AddScssBundle("/css/darkmode-initial.css", "/css/darkmode-initial.scss");
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app
				.UseRobots()
				.UseMiddleware<Middleware.HtmlRedirectionMiddleware>()
				.UseRequestLocalization()
				.UseExceptionHandlers(env)
				.UseGzipCompression(Settings)
				.UseWebOptimizer()
				.UseStaticFilesWithExtensionMapping()
				.UseAuthorization()
				.UseAuthentication()
				.UseSwaggerUi(Environment)
				.UseLogging()
				.UseMvcWithOptions();

			if (env.IsDevelopment())
			{
				app.UseHttpsRedirection();
			}
		}
	}
}
