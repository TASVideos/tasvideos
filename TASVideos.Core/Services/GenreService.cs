using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Services;

public record GenreDto(int Id, string DisplayName, int GameCount);

public enum GenreChangeResult { Success, Fail, NotFound, InUse }

public interface IGenreService
{
	ValueTask<ICollection<GenreDto>> GetAll();
	ValueTask<GenreDto?> GetById(int id);
	Task<bool> InUse(int id);
	Task<int?> Add(string displayName);
	Task<GenreChangeResult> Edit(int id, string displayName);
	Task<GenreChangeResult> Delete(int id);
}

internal class GenreService : IGenreService
{
	internal const string CacheKey = "AllGameGenres";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;
	private readonly ILogger<GenreService> _logger;

	public GenreService(ApplicationDbContext db, ICacheService cache, ILogger<GenreService> logger)
	{
		_db = db;
		_cache = cache;
		_logger = logger;
	}

	public async ValueTask<ICollection<GenreDto>> GetAll()
	{
		if (_cache.TryGetValue(CacheKey, out List<GenreDto> genres))
		{
			return genres;
		}

		genres = await _db.Genres
			.Select(g => new GenreDto(g.Id, g.DisplayName, g.GameGenres.Count))
			.ToListAsync();
		_cache.Set(CacheKey, genres);
		return genres;
	}

	public async ValueTask<GenreDto?> GetById(int id)
	{
		var genre = await GetAll();
		return genre.SingleOrDefault(g => g.Id == id);
	}

	public async Task<bool> InUse(int id)
	{
		return await _db.GameGenres.AnyAsync(gg => gg.GenreId == id);
	}

	public async Task<int?> Add(string displayName)
	{
		var entry = _db.Genres.Add(new Genre
		{
			DisplayName = displayName
		});

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(CacheKey);
			return entry.Entity.Id;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError("Unable to create genre {displayName}: {ex}", displayName, ex);
			return null;
		}
	}

	public async Task<GenreChangeResult> Edit(int id, string displayName)
	{
		var genre = await _db.Genres.SingleOrDefaultAsync(t => t.Id == id);
		if (genre is null)
		{
			return GenreChangeResult.NotFound;
		}

		genre.DisplayName = displayName;

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(CacheKey);
			return GenreChangeResult.Success;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError("Unable to edit genre {displayName}: {ex}", displayName, ex);
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
			var genre = await _db.Genres.SingleOrDefaultAsync(g => g.Id == id);
			if (genre is null)
			{
				return GenreChangeResult.NotFound;
			}

			_db.Genres.Remove(genre);
			await _db.SaveChangesAsync();
			_cache.Remove(CacheKey);
			return GenreChangeResult.Success;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError("Unable to delete genre with id {id}: {ex}", id, ex);
			return GenreChangeResult.Fail;
		}
	}
}
