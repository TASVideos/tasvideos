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

public class MovieTokens : IPublicationTokens
{
	public ICollection<string> SystemCodes { get; init; } = [];
	public ICollection<string> Classes { get; init; } = [];
	public ICollection<int> Years { get; init; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1).ToList();
	public ICollection<string> Tags { get; init; } = [];
	public ICollection<string> Genres { get; init; } = [];
	public ICollection<string> Flags { get; init; } = [];

	public bool ShowObsoleted { get; set; }
	public bool OnlyObsoleted { get; set; }
	public string SortBy { get; set; } = "";
	public int? Limit { get; set; }

	public ICollection<int> Authors { get; init; } = [];

	public ICollection<int> MovieIds { get; init; } = [];

	public ICollection<int> Games { get; init; } = [];
	public ICollection<int> GameGroups { get; init; } = [];
}
