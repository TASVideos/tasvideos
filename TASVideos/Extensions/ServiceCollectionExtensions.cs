using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TASVideos.Api.Controllers;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.MovieParsers;
using TASVideos.Pages;
using TASVideos.Services;
using TASVideos.Services.Email;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Services.PublicationChain;
using TASVideos.Services.RssFeedParsers;

namespace TASVideos.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<AppSettings>(configuration);
			return services;
		}

		public static IServiceCollection AddRequestLocalization(this IServiceCollection services)
		{
			services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new RequestCulture("en-US");
				options.SupportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			});
			return services;
		}

		public static IServiceCollection AddCookieConfiguration(this IServiceCollection services, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				services.ConfigureApplicationCookie(options =>
				{
					options.ExpireTimeSpan = TimeSpan.FromDays(90);
				});
			}

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

		public static IServiceCollection AddCacheService(this IServiceCollection services, AppSettings.CacheSetting cacheSettings)
		{
			if (cacheSettings.CacheType == "Memory")
			{
				services.AddMemoryCache();
				services.AddSingleton<ICacheService, MemoryCacheService>();
			}
			else
			{
				services.AddSingleton<ICacheService, NoCacheService>();
			}

			return services;
		}

		public static IServiceCollection AddServices(this IServiceCollection services, IWebHostEnvironment env)
		{
			services.AddScoped<UserManager>();
			services.AddScoped<IPointsService, PointsService>();
			services.AddScoped<IAwards, Awards>();
			services.AddScoped<IMediaFileUploader, MediaFileUploader>();
			services.AddScoped<ILanguages, Languages>();
			services.AddScoped<ITopicWatcher, TopicWatcher>();
			services.AddScoped<IPublicationHistory, PublicationHistory>();
			services.AddScoped<IFileService, FileService>();
			services.AddScoped<IMovieSearchTokens, MovieSearchTokens>();
			services.AddScoped<IVcsRssParser, VcsRssParser>();
			services.AddScoped<IIpBanService, IpBanService>();
			services.AddScoped<ITagService, TagService>();
			services.AddScoped<IFlagService, FlagService>();

			if (env.IsDevelopment())
			{
				services.AddTransient<IEmailSender, EmailLogger>();
			}
			else
			{
				services.AddTransient<IEmailSender, SendGridSender>();
			}

			services.AddTransient<IEmailService, EmailService>();
			services.AddTransient<IWikiPages, WikiPages>();

			services.AddScoped<ITASVideoAgent, TASVideoAgent>();
			services.AddScoped<ITASVideosGrue, TASVideosGrue>();

			services.AddScoped<ForumEngine.IWriterHelper, ForumWriterHelper>();

			return services;
		}

		public static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<IMovieParser, MovieParser>();
			return services;
		}

		public static IServiceCollection AddMvcWithOptions(this IServiceCollection services, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				services.AddDatabaseDeveloperPageExceptionFilter();
			}

			services.AddResponseCaching();
			services
				.AddControllers()
				.AddNewtonsoftJson();
			services
				.AddRazorPages(options =>
				{
					options.Conventions.AddPageRoute("/Wiki/Render", "{*url}");
					options.Conventions.AddFolderApplicationModelConvention(
						"/",
						model => model.Filters.Add(new SetPageViewBagAttribute()));
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
					options.Conventions.AddPageRoute("/Submissions/LegacyQueue", "queue.cgi");
					options.Conventions.AddPageRoute("/Publications/LegacyMovies", "movies.cgi");
				})
				.AddRazorRuntimeCompilation()
				.AddApplicationPart(typeof(PublicationsController).Assembly);

			services.AddHttpContext();
			services.AddMvc(options => options.ValueProviderFactories.AddDelimitedValueProviderFactory('|'));
			return services;
		}

		public static IServiceCollection AddIdentity(this IServiceCollection services, IWebHostEnvironment env)
		{
			services.AddIdentity<User, Role>(config =>
				{
					config.SignIn.RequireConfirmedEmail = !env.IsDevelopment();
					config.Password.RequiredLength = 12;
					config.Password.RequireDigit = false;
					config.Password.RequireLowercase = false;
					config.Password.RequireNonAlphanumeric = false;
					config.Password.RequiredUniqueChars = 4;
					config.User.RequireUniqueEmail = true;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			return services;
		}

		public static IServiceCollection AddAutoMapperWithProjections(this IServiceCollection services)
		{
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			return services;
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

		internal static IServiceCollection AddExternalMediaPublishing(this IServiceCollection services, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				services.AddSingleton<IPostDistributor, ConsoleDistributor>();
			}

			services.AddSingleton<IPostDistributor, IrcDistributor>();
			services.AddSingleton<IPostDistributor, DiscordDistributor>();
			services.AddSingleton<IPostDistributor, TwitterDistributor>();
			services.AddScoped<IPostDistributor, DistributorStorage>();

			services.AddTransient<ExternalMediaPublisher>();

			return services;
		}

		private static IServiceCollection AddHttpContext(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient(
				provider => provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

			return services;
		}
	}
}
