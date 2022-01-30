using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.MovieParsers;
using TASVideos.Pages;
using TASVideos.Services;

namespace TASVideos.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<AppSettings>(configuration);
		var settings = configuration.Get<AppSettings>();
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

	public static IServiceCollection AddMovieParser(this IServiceCollection services)
	{
		return services.AddSingleton<IMovieParser, MovieParser>();
	}

	public static IServiceCollection AddMvcWithOptions(this IServiceCollection services, IHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			services.AddDatabaseDeveloperPageExceptionFilter();
		}

		services.AddResponseCaching();
		services
			.AddRazorPages(options =>
			{
				options.Conventions.AddPageRoute("/Wiki/Render", "{*url}");
				options.Conventions.AddFolderApplicationModelConvention(
					"/",
					model => model.Filters.Add(new SetPageViewBagAttribute()));
				options.Conventions.AddFolderApplicationModelConvention(
					"/",
					model => model.Filters.Add(new Debouncer()));
				options.Conventions.AddPageRoute("/Games/Index", "{id:int}G");
				options.Conventions.AddPageRoute("/Submissions/Index", "Subs-{query}");
				options.Conventions.AddPageRoute("/Submissions/Index", "Subs-List");
				options.Conventions.AddPageRoute("/Submissions/View", "{id:int}S");
				options.Conventions.AddPageRoute("/Publications/Index", "Movies-{query}");
				options.Conventions.AddPageRoute("/Publications/View", "{id:int}M");
				options.Conventions.AddPageRoute("/Publications/Authors", "Players-List");
				options.Conventions.AddPageRoute("/Forum/Posts/Index", "forum/p/{id:int}");
				options.Conventions.AddPageRoute("/Submissions/Submit", "SubmitMovie");
				options.Conventions.AddPageRoute("/Forum/MoodReport", "forum/moodreport.php");
				options.Conventions.AddPageRoute("/Permissions/Index", "/Privileges");

				// Backwards compatibility with legacy links
				options.Conventions.AddPageRoute("/Forum/Legacy/Topic", "forum/viewtopic.php");
				options.Conventions.AddPageRoute("/Forum/Legacy/Topic", "forum/t/{id:int}");
				options.Conventions.AddPageRoute("/Forum/Legacy/Forum", "forum/viewforum.php");
				options.Conventions.AddPageRoute("/Forum/Legacy/Forum", "forum/f/{id:int}");
				options.Conventions.AddPageRoute("/Submissions/LegacyQueue", "queue.cgi");
				options.Conventions.AddPageRoute("/Publications/LegacyMovies", "movies.cgi");
				options.Conventions.AddPageRoute("/RamAddresses/LegacyList", "AddressesUp");

				options.Conventions.AddPageRoute("/RamAddresses/List", "Addresses-List");
				options.Conventions.AddPageRoute("/RamAddresses/Index", "Addresses-{id:int}");

				options.Conventions.AddPageRoute("/RssFeeds/Publications", "/publications.rss");
				options.Conventions.AddPageRoute("/RssFeeds/Submissions", "/submissions.rss");
				options.Conventions.AddPageRoute("/RssFeeds/Wiki", "/wiki.rss");
				options.Conventions.AddPageRoute("/RssFeeds/News", "/news.rss");
			})
			.AddRazorRuntimeCompilation();

		services.AddHttpContext();
		services.AddMvc(options => options.ValueProviderFactories.AddDelimitedValueProviderFactory('|'));

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
		services.AddIdentity<User, Role>(config =>
			{
				config.SignIn.RequireConfirmedEmail = env.IsProduction() || env.IsStaging();
				config.Password.RequiredLength = 12;
				config.Password.RequireDigit = false;
				config.Password.RequireLowercase = false;
				config.Password.RequireNonAlphanumeric = false;
				config.Password.RequiredUniqueChars = 4;
				config.User.RequireUniqueEmail = true;
				config.User.AllowedUserNameCharacters += "āâãáéëöú£ "; // The space is intentional
				})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

		return services;
	}

	public static IServiceCollection AddAutoMapperWithProjections(this IServiceCollection services)
	{
		return services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
	}

	public static IServiceCollection AddSwagger(this IServiceCollection services, AppSettings settings)
	{
		services.AddAuthentication(x =>
		{
			x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(x =>
		{
			x.RequireHttpsMetadata = true;
			x.SaveToken = true;
			x.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Jwt.SecretKey)),
				ValidateIssuer = false,
				ValidateAudience = false
			};
		});

		var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

		return services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc(
				"v1",
				new OpenApiInfo
				{
					Title = "TASVideos API",
					Version = $"v{version.Major}.{version.Minor}.{version.Revision}",
					Description = "API For tasvideos.org content"
				});
			c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
			{
				Name = "Authorization"
			});
			var basePath = AppContext.BaseDirectory;
			var xmlPath = Path.Combine(basePath, "TASVideos.Api.xml");
			c.IncludeXmlComments(xmlPath);
		});
	}

	private static IServiceCollection AddHttpContext(this IServiceCollection services)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		return services.AddTransient(
			provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);
	}
}
