using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.MovieParsers;
using TASVideos.Tasks;

namespace TASVideos.Extensions
{
	public static class ServiceCollectionExtensions
	{
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
	}
}
