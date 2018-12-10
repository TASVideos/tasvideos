using System;
using System.IO.Compression;

using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.MovieParsers;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Tasks;

namespace TASVideos.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<AppSettings>(configuration);
			return services;
		}

		public static IServiceCollection AddCookieConfiguration(this IServiceCollection services, IHostingEnvironment env)
		{
			if (env.IsAnyTestEnvironment())
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

		public static IServiceCollection AddTasks(this IServiceCollection services)
		{
			services.AddScoped<AwardTasks>();
			services.AddScoped<PermissionTasks>();
			services.AddScoped<UserTasks>();
			services.AddScoped<RoleTasks>();
			services.AddScoped<WikiTasks>();
			services.AddScoped<SubmissionTasks>();
			services.AddScoped<PublicationTasks>();
			services.AddScoped<PlatformTasks>();
			services.AddScoped<CatalogTasks>();
			services.AddScoped<GameTasks>();
			services.AddScoped<ForumTasks>();
			services.AddScoped<RatingsTasks>();
			services.AddScoped<PrivateMessageTasks>();
			services.AddScoped<UserFileTasks>();
			services.AddScoped<MediaTasks>();

			return services;
		}

		public static IServiceCollection AddServices(this IServiceCollection services)
		{
			services.AddScoped<IFileService, FileService>();
			services.AddScoped<IPointsService, PointsService>();
			services.AddTransient<IEmailSender, EmailSender>();
			services.AddTransient<IEmailService, EmailService>();

			return services;
		}

		public static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<MovieParser>();
			return services;
		}

		public static IServiceCollection AddWikiProvider(this IServiceCollection services)
		{
			var provider = new Razor.WikiMarkupFileProvider();
			services.AddSingleton(provider);
			services.Configure<RazorViewEngineOptions>(
				opts => opts.FileProviders.Add(provider));

			return services;
		}

		public static IServiceCollection AddMvcWithOptions(this IServiceCollection services)
		{
			services.AddResponseCaching();
			services.AddMvc(options =>
			{
				options.Filters.Add(new SetViewBagAttribute());
			});

			services.AddHttpContext();

			return services;
		}

		public static IServiceCollection AddIdentity(this IServiceCollection services)
		{
			services.AddIdentity<User, Role>(config =>
				{
					config.SignIn.RequireConfirmedEmail = true;
					config.Password.RequiredLength = 12;
					config.Password.RequireDigit = false;
					config.Password.RequireLowercase = false;
					config.Password.RequireNonAlphanumeric = false;
					config.Password.RequiredUniqueChars = 4;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			return services;
		}

		public static IServiceCollection AddAutoMapperWithProjections(this IServiceCollection services)
		{
			Mapper.Initialize(cfg => cfg.AddProfile(new MappingProfile()));
			services.AddAutoMapper();
			  
			return services;
		}

		private static IServiceCollection AddHttpContext(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient(
				provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

			return services;
		}

		internal static IServiceCollection AddExternalMediaPublishing(this IServiceCollection services, IHostingEnvironment env, AppSettings settings)
		{
			if (env.IsAnyTestEnvironment())
			{
				services.AddSingleton<IPostDistributor, ConsoleDistributor>();
			}

			services.AddScoped<IPostDistributor, DistributorStorage>();

			services.AddTransient<ExternalMediaPublisher>();
			
			return services;
		}
	}
}
