using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.MovieParsers;

public static class ServiceCollectionExtensions
{
	[RequiresUnreferencedCode(nameof(MovieParser))]
	public static IServiceCollection AddTasvideosMovieParsers(this IServiceCollection services)
	{
		return services.AddSingleton<IMovieParser, MovieParser>();
	}
}
