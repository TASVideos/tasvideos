using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using TASVideos.Core.Settings;
using TASVideos.MovieParsers;
using TASVideos.Pages;
using TASVideos.Services;

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
		new ("/RssFeeds/News", "/news.rss")
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
				options.Conventions.AddFolderApplicationModelConvention(
					"/",
					model => model.Filters.Add(new SetPageViewBagAttribute()));

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

		return services;
	}

	public static IServiceCollection AddTextModules(this IServiceCollection services)
	{
		foreach (var component in ModuleParamHelpers.TextComponents.Values)
		{
			services.AddScoped(component);
		}

		return services;
	}

	public static IServiceCollection AddIdentity(this IServiceCollection services, IHostEnvironment env)
	{
		services.Configure<PasswordHasherOptions>(options => options.IterationCount = 720_000);
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
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		return services.AddTransient(
			provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);
	}
}
