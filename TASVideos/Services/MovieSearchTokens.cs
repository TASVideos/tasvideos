using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services.Dtos;

namespace TASVideos.Services
{
	public interface IMovieSearchTokens
	{
		ValueTask<IPublicationTokens> GetTokens();
	}

	public class MovieSearchTokens : IMovieSearchTokens
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public MovieSearchTokens(ApplicationDbContext db, ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async ValueTask<IPublicationTokens> GetTokens()
		{
			if (_cache.TryGetValue(CacheKeys.MovieTokens, out MovieTokens cachedResult))
			{
				return cachedResult;
			}

			var result = new MovieTokens
			{
				Tiers = await _db.Tiers.Select(t => t.Name.ToLower()).ToListAsync(),
				SystemCodes = await _db.GameSystems.Select(s => s.Code.ToLower()).ToListAsync(),
				Tags = await _db.Tags.Select(t => t.Code.ToLower()).ToListAsync(),
				Genres = await _db.Genres.Select(g => g.DisplayName.ToLower()).ToListAsync(),
				Flags = await _db.Flags.Select(f => f.Token.ToLower()).ToListAsync()
			};

			_cache.Set(CacheKeys.MovieTokens, result);

			return result;
		}
	}
}
