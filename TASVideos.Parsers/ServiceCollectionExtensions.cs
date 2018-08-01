using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.MovieParsers
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTasvideosMovieParsers(this IServiceCollection services)
		{
			services.AddMovieParser();
			return services;
		}

		internal static IServiceCollection AddMovieParser(this IServiceCollection services)
		{
			services.AddSingleton<MovieParser>();
			return services;
		}
	}
}
