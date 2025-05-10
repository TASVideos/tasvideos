using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using OpenTelemetry.Metrics;
using TASVideos.Core.Settings;
using TASVideos.TagHelpers;

namespace TASVideos.Extensions;

public static class ServiceCollectionExtensions
{
	// TODO: move these to a more appropriate place
	public static readonly List<KeyValuePair<string, string>> Aliases =
	[
		new("/Games/Index", "{id:int}G"),
		new("/Submissions/Index", "Subs-{query}"),
		new("/Submissions/Index", "Subs-List"),
		new("/Submissions/View", "{id:int}S"),
		new("/Publications/Index", "Movies-{query}"),
		new("/Publications/View", "{id:int}M"),
		new("/RssFeeds/Publications", "/publications.rss"),
		new("/RssFeeds/Submissions", "/submissions.rss"),
		new("/RssFeeds/Wiki", "/wiki.rss"),
		new("/RssFeeds/News", "/news.rss")
	];

	public static readonly List<KeyValuePair<string, string>> LegacyRedirects =
	[
		new("/Forum/Legacy/Topic", "forum/viewtopic.php"),
		new("/Forum/Legacy/Topic", "forum/t/{id:int}"),
		new("/Forum/Legacy/Post", "forum/p/{id:int}"),
		new("/Forum/Legacy/Forum", "forum/viewforum.php"),
		new("/Forum/Legacy/Forum", "forum/f/{id:int}"),
		new("/Submissions/LegacyQueue", "queue.cgi"),
		new("/Publications/LegacyMovies", "movies.cgi"),
		new("/Forum/Legacy/MoodReport", "forum/moodreport.php"),
		new("/Wiki/Legacy/Privileges", "Privileges"),
		new("/Wiki/Legacy/SubmitMovie", "SubmitMovie")
	];

	public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<AppSettings>(configuration);
		var settings = configuration.Get<AppSettings>()!;
		return services.AddSingleton(settings);
	}

	public static IServiceCollection AddRequestLocalization(this IServiceCollection services)
	{
		return services.Configure<RequestLocalizationOptions>(options =>
		{
			options.DefaultRequestCulture = new RequestCulture("en-US");
			options.SupportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
		});
	}

	public static IServiceCollection AddCookieConfiguration(this IServiceCollection services)
	{
		services.ConfigureApplicationCookie(options =>
		{
			options.ExpireTimeSpan = TimeSpan.FromDays(90);
		});

		return services;
	}

	public static IServiceCollection AddGzipCompression(this IServiceCollection services, AppSettings settings)
	{
		if (settings.EnableGzipCompression)
		{
			services.Configure<GzipCompressionProviderOptions>(options =>
			{
				options.Level = CompressionLevel.Fastest;
			});
			services.AddResponseCompression(options => options.EnableForHttps = true);
		}

		return services;
	}

	public static IServiceCollection AddRazorPages(this IServiceCollection services, IHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			services.AddDatabaseDeveloperPageExceptionFilter();
		}

		services.AddResponseCaching();
		var pagesResult = services
			.AddRazorPages(options =>
			{
				options.Conventions.AddPageRoute("/Wiki/Render", "{*url}");

				foreach (var alias in Aliases)
				{
					options.Conventions.AddPageRoute(alias.Key, alias.Value);
				}

				foreach (var redirect in LegacyRedirects)
				{
					options.Conventions.AddPageRoute(redirect.Key, redirect.Value);
				}
			});

		if (!env.IsProduction())
		{
			pagesResult.AddRazorRuntimeCompilation();
		}

		services.AddAntiforgery(options =>
		{
			options.Cookie.SameSite = SameSiteMode.Lax;
		});

		services.AddHttpContext();
		services.AddMvc(options =>
		{
			options.ValueProviderFactories.AddDelimitedValueProviderFactory('|');
			options.ModelBinderProviders.Insert(0, new TrimStringModelBinderProvider());
		});

		services.AddSingleton<IHtmlGenerator, OverrideHtmlGenerator>();
		return services;
	}

	public static IServiceCollection AddServices(this IServiceCollection services)
	{
		foreach (var component in ModuleParamHelpers.TextComponents.Values)
		{
			services.AddScoped(component);
		}

		return services.AddTransient<IExternalMediaPublisher, ExternalMediaPublisher>();
	}

	public static IServiceCollection AddIdentity(this IServiceCollection services, IHostEnvironment env)
	{
		services.Configure<PasswordHasherOptions>(options => options.IterationCount = 720_000);
		services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>(); // the default would use UserClaimsPrincipalFactory<User, Role>, but like this we prevent it putting roles in the principal and thus in the identity cookie
		services.AddIdentity<User, Role>(config =>
			{
				config.SignIn.RequireConfirmedEmail = env.IsProduction() || env.IsStaging();
				config.Password.RequiredLength = 12;
				config.Password.RequireDigit = false;
				config.Password.RequireLowercase = false;
				config.Password.RequireNonAlphanumeric = false;
				config.Password.RequiredUniqueChars = 4;
				config.User.RequireUniqueEmail = true;
				config.User.AllowedUserNameCharacters += "āàâãáäéèëêíîïóôöúüûý£ŉçÑñ";
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

		return services;
	}

	private static IServiceCollection AddHttpContext(this IServiceCollection services)
	{
		return services
			.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
			.AddTransient(provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);
	}

	public static IServiceCollection AddMetrics(this IServiceCollection services, AppSettings settings)
	{
		if (settings.EnableMetrics)
		{
			services
				.AddOpenTelemetry()
				.WithMetrics(builder =>
				{
					builder.AddMeter("Microsoft.AspNetCore.Hosting")
						.AddView("http.server.request.duration", new ExplicitBucketHistogramConfiguration
						{
							Boundaries = [] // disable duration histograms of endpoints, which is a LOT of data, but keep total counts
						});

					builder.AddMeter(
						"Microsoft.AspNetCore.Server.Kestrel",
						"Microsoft.AspNetCore.Routing",
						"Microsoft.AspNetCore.Diagnostics");

					builder.AddMeter("Npgsql")
						.AddView("db.client.commands.duration", new ExplicitBucketHistogramConfiguration
						{
							Boundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
						})
						.AddView("db.client.connections.create_time", new ExplicitBucketHistogramConfiguration
						{
							Boundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
						});

					builder.AddMeter("TASVideos");

					builder.AddPrometheusExporter(options =>
					{
						options.ScrapeEndpointPath = "/Metrics";
					});
				});

			services.AddSingleton<ITASVideosMetrics, TASVideosMetrics>();
		}
		else
		{
			services.AddSingleton<ITASVideosMetrics, NullMetrics>();
		}

		return services;
	}
}
