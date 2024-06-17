using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services;

public record GenreDto(int Id, string DisplayName, int GameCount);

public enum GenreChangeResult { Success, Fail, NotFound, InUse }

public interface IGenreService
{
	ValueTask<IReadOnlyCollection<GenreDto>> GetAll();
	ValueTask<GenreDto?> GetById(int id);
	Task<bool> InUse(int id);
	Task<int?> Add(string displayName);
	Task<GenreChangeResult> Edit(int id, string displayName);
	Task<GenreChangeResult> Delete(int id);
}

internal class GenreService(ApplicationDbContext db, ICacheService cache, ILogger<GenreService> logger)
	: IGenreService
{
	internal const string CacheKey = "AllGameGenres";

	public async ValueTask<IReadOnlyCollection<GenreDto>> GetAll()
	{
		if (cache.TryGetValue(CacheKey, out List<GenreDto> genres))
		{
			return genres;
		}

		genres = await db.Genres
			.Select(g => new GenreDto(g.Id, g.DisplayName, g.GameGenres.Count))
			.ToListAsync();
		cache.Set(CacheKey, genres);
		return genres;
	}

	public async ValueTask<GenreDto?> GetById(int id)
	{
		var genre = await GetAll();
		return genre.SingleOrDefault(g => g.Id == id);
	}

	public async Task<bool> InUse(int id) => await db.GameGenres.AnyAsync(gg => gg.GenreId == id);

	public async Task<int?> Add(string displayName)
	{
		var entry = db.Genres.Add(new Genre
		{
			DisplayName = displayName
		});

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(CacheKey);
			return entry.Entity.Id;
		}
		catch (DbUpdateException ex)
		{
			logger.LogError("Unable to create genre {displayName}: {ex}", displayName, ex);
			return null;
		}
	}

	public async Task<GenreChangeResult> Edit(int id, string displayName)
	{
		var genre = await db.Genres.FindAsync(id);
		if (genre is null)
		{
			return GenreChangeResult.NotFound;
		}

		genre.DisplayName = displayName;

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(CacheKey);
			return GenreChangeResult.Success;
		}
		catch (DbUpdateException ex)
		{
			logger.LogError("Unable to edit genre {displayName}: {ex}", displayName, ex);
			return GenreChangeResult.Fail;
		}
	}

	public async Task<GenreChangeResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return GenreChangeResult.InUse;
		}

		try
		{
			var genre = await db.Genres.SingleOrDefaultAsync(g => g.Id == id);
			if (genre is null)
			{
				return GenreChangeResult.NotFound;
			}

			db.Genres.Remove(genre);
			await db.SaveChangesAsync();
			cache.Remove(CacheKey);
			return GenreChangeResult.Success;
		}
		catch (DbUpdateException ex)
		{
			logger.LogError("Unable to delete genre with id {id}: {ex}", id, ex);
			return GenreChangeResult.Fail;
		}
	}
}
