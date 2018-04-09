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
			services.AddScoped<AwardTasks, AwardTasks>();
			services.AddScoped<PermissionTasks, PermissionTasks>();
			services.AddScoped<UserTasks, UserTasks>();
			services.AddScoped<RoleTasks, RoleTasks>();
			services.AddScoped<WikiTasks, WikiTasks>();
			services.AddScoped<SubmissionTasks, SubmissionTasks>();
			services.AddScoped<PublicationTasks, PublicationTasks>();
			services.AddScoped<PlatformTasks, PlatformTasks>();
			services.AddScoped<CatalogTasks, CatalogTasks>();
			services.AddScoped<GameTasks, GameTasks>();
			services.AddScoped<ForumTasks, ForumTasks>();

			return services;
		}

		public static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<MovieParser, MovieParser>();
			return services;
		}

		public static IServiceCollection AddWikiProvider(this IServiceCollection services)
		{
			var provider = new Razor.WikiMarkupFileProvider(services.BuildServiceProvider());
			services.AddSingleton(provider);
			services.Configure<RazorViewEngineOptions>(
				opts => opts.FileProviders.Add(provider));

			return services;
		}
	}
}
