namespace TASVideos.Core.Services;

public interface IMovieSearchTokens
{
	ValueTask<IPublicationTokens> GetTokens();
}

internal class MovieSearchTokens(ApplicationDbContext db, ICacheService cache) : IMovieSearchTokens
{
	public async ValueTask<IPublicationTokens> GetTokens()
	{
		if (cache.TryGetValue(CacheKeys.MovieTokens, out MovieTokens cachedResult))
		{
			return cachedResult;
		}

		var result = new MovieTokens
		{
			Classes = await db.PublicationClasses.Select(t => t.Name.ToLower()).ToListAsync(),
			SystemCodes = await db.GameSystems.Select(s => s.Code.ToLower()).ToListAsync(),
			Tags = await db.Tags.Select(t => t.Code.ToLower()).ToListAsync(),
			Genres = await db.Genres.Select(g => g.DisplayName.ToLower()).ToListAsync(),
			Flags = await db.Flags.Select(f => f.Token.ToLower()).ToListAsync()
		};

		cache.Set(CacheKeys.MovieTokens, result);

		return result;
	}
}
