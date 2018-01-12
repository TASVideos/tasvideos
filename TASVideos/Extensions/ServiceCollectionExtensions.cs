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
			services.AddScoped<PermissionTasks, PermissionTasks>();
			services.AddScoped<UserTasks, UserTasks>();
			services.AddScoped<RoleTasks, RoleTasks>();
			services.AddScoped<WikiTasks, WikiTasks>();
			services.AddScoped<SubmissionTasks, SubmissionTasks>();
			services.AddScoped<PlatformTasks, PlatformTasks>();

			return services;
		}

		public static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<MovieParser, MovieParser>();
			return services;
		}

		public static IServiceCollection AddWikiProvider(this IServiceCollection services)
		{
			services.Configure<RazorViewEngineOptions>(
				opts => opts.FileProviders.Add(new Razor.WikiMarkupFileProvider(services.BuildServiceProvider())));

			return services;
		}
	}
}
