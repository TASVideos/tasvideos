using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddGzipCompression(this IServiceCollection services)
		{
			services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
			services.AddResponseCompression();

			return services;
		}

		public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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
			services.AddScoped<PointsService>();

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

		public static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<MovieParser>();
			return services;
		}

		public static IServiceCollection AddHttpContext(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient(
				provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

			return services;
		}

		public static IServiceCollection AddFileService(this IServiceCollection services)
		{
			services.AddScoped<IFileService, FileService>();
			return services;
		}

		public static IServiceCollection AddPointsService(this IServiceCollection services)
		{
			services.AddScoped<IPointsService, PointsService>();
			return services;
		}

		public static IServiceCollection AddEmailService(this IServiceCollection services)
		{
			services.AddTransient<IEmailSender, EmailSender>();
			return services;
		}
	}
}
