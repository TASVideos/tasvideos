using Microsoft.Extensions.Logging.Abstractions;
using TASVideos.Core.Services;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class GenreServicesTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly GenreService _genreService;

	public GenreServicesTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_genreService = new GenreService(_db, _cache, new NullLogger<GenreService>());
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
	{
		var result = await _genreService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task GetAll_CachesAndReturns()
	{
		const int genreId = 1;
		const int gameId = 2;
		_db.Add(new Genre { Id = genreId });
		_db.Add(new Game { Id = gameId });
		_db.Add(new GameGenre { GameId = gameId, GenreId = gameId });
		await _db.SaveChangesAsync();

		var result = await _genreService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		var genre = result.Single();
		Assert.AreEqual(genreId, genre.Id);
		Assert.AreEqual(1, genre.GameCount);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_CachesAndReturnsNull()
	{
		var result = await _genreService.GetById(-1);
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsGenre()
	{
		const int genreId = 1;
		const int gameId = 2;
		const string displayName = "Test";
		_db.Genres.Add(new Genre { Id = genreId, DisplayName = displayName });
		_db.Add(new Game { Id = gameId });
		_db.Add(new GameGenre { GameId = gameId, GenreId = gameId });
		await _db.SaveChangesAsync();

		var result = await _genreService.GetById(genreId);
		Assert.IsNotNull(result);
		Assert.AreEqual(displayName, result.DisplayName);
		Assert.AreEqual(1, result.GameCount);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task InUse_DoesNotExist_ReturnsFalse()
	{
		var result = await _genreService.InUse(-1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InUse_Exists_ReturnsTrue()
	{
		const int genreId = 1;
		const int gameId = 1;
		_db.Genres.Add(new Genre { Id = genreId });
		_db.Games.Add(new Game { Id = gameId });
		_db.GameGenres.Add(new GameGenre { GameId = gameId, GenreId = genreId });
		await _db.SaveChangesAsync();

		var result = await _genreService.InUse(genreId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Add_Success_FlushesCaches()
	{
		const string displayName = "Display";

		var result = await _genreService.Add(displayName);

		Assert.IsNotNull(result);
		Assert.AreEqual(1, _db.Genres.Count());
		var genre = _db.Genres.Single();
		Assert.AreEqual(displayName, genre.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_DoesNotFlushCache()
	{
		_cache.Set(GenreService.CacheKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _genreService.Add("Test");
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Edit_Success_FlushesCaches()
	{
		const int id = 1;
		const string newDisplayName = "Display";
		_db.Genres.Add(new Genre { Id = id });
		await _db.SaveChangesAsync();

		var result = await _genreService.Edit(id, newDisplayName);

		Assert.AreEqual(GenreChangeResult.Success, result);
		Assert.AreEqual(1, _db.Genres.Count());
		var genre = _db.Genres.Single();
		Assert.AreEqual(newDisplayName, genre.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Edit_NotFound_DoesNotFlushCache()
	{
		const int id = 1;
		_db.Genres.Add(new Genre { Id = id });
		await _db.SaveChangesAsync();
		_cache.Set(GenreService.CacheKey, new object());

		var result = await _genreService.Edit(id + 1, "Test");

		Assert.AreEqual(GenreChangeResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Edit_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		_db.Genres.Add(new Genre { Id = id });
		_cache.Set(GenreService.CacheKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _genreService.Edit(id, "DisplayName");
		Assert.AreEqual(GenreChangeResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Delete_Success_FlushesCache()
	{
		const int id = 1;
		var genre = new Genre { Id = id };
		_db.Genres.Add(genre);
		await _db.SaveChangesAsync();

		var result = await _genreService.Delete(id);
		Assert.AreEqual(GenreChangeResult.Success, result);
		Assert.IsFalse(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Delete_NotFound_DoesNotFlushCache()
	{
		_cache.Set(GenreService.CacheKey, new object());

		var result = await _genreService.Delete(-1);
		Assert.AreEqual(GenreChangeResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Delete_InUse_DoesNotFlushCache()
	{
		const int genreId = 1;
		const int gameId = 1;
		var genre = new Genre { Id = genreId };
		_db.Genres.Add(genre);
		_cache.Set(GenreService.CacheKey, new object());
		_db.Games.Add(new Game { Id = gameId });
		_db.GameGenres.Add(new GameGenre { GameId = gameId, GenreId = genreId });
		await _db.SaveChangesAsync();

		var result = await _genreService.Delete(genreId);
		Assert.AreEqual(GenreChangeResult.InUse, result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}

	[TestMethod]
	public async Task Delete_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		_db.Genres.Add(new Genre { Id = 1 });
		await _db.SaveChangesAsync();
		_cache.Set(GenreService.CacheKey, new object());
		_db.CreateConcurrentUpdateConflict();

		var result = await _genreService.Delete(id);
		Assert.AreEqual(GenreChangeResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(GenreService.CacheKey));
	}
}
