using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.MovieParsers;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTasvideosMovieParsers(this IServiceCollection services)
	{
		return services.AddSingleton<IMovieParser, MovieParser>();
	}
}
