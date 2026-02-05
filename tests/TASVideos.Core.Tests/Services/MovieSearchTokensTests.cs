using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class MovieSearchTokensTests : TestDbBase
{
	private readonly TestCache _testCache;
	private readonly MovieSearchTokens _movieSearchTokens;

	public MovieSearchTokensTests()
	{
		_testCache = new TestCache();
		_movieSearchTokens = new MovieSearchTokens(_db, _testCache);
	}

	[TestMethod]
	public async Task GetTokens_FirstCall_LoadsFromDatabaseAndCaches()
	{
		_db.Add(new GameSystem { Id = 1, Code = "TestSystem" });
		_db.Add(new PublicationClass { Id = 1, Name = "TestClass" });
		_db.Add(new Tag { Id = 1, Code = "TestTag" });
		_db.Add(new Genre { Id = 1, DisplayName = "TestGenre" });
		_db.Add(new Flag { Id = 1, Name = "TestFlag", Token = "test-token" });
		await _db.SaveChangesAsync();

		var result = await _movieSearchTokens.GetTokens();

		Assert.IsNotNull(result);
		Assert.Contains("testsystem", result.SystemCodes);
		Assert.Contains("testclass", result.Classes);
		Assert.Contains("testtag", result.Tags);
		Assert.Contains("testgenre", result.Genres);
		Assert.Contains("test-token", result.Flags);

		Assert.IsTrue(_testCache.ContainsKey(CacheKeys.MovieTokens));
		Assert.AreEqual(1, _testCache.Count());
	}

	[TestMethod]
	public async Task GetTokens_SecondCall_ReturnsFromCache()
	{
		_db.Add(new GameSystem { Id = 1, Code = "CachedSystem" });
		_db.Add(new PublicationClass { Id = 1, Name = "CachedClass" });
		await _db.SaveChangesAsync();
		await _movieSearchTokens.GetTokens(); // Prime the cache

		// Modify database data after caching
		_db.Add(new GameSystem { Id = 2, Code = "NewSystem" });
		_db.Add(new PublicationClass { Id = 2, Name = "NewClass" });
		await _db.SaveChangesAsync();

		var result = await _movieSearchTokens.GetTokens();

		Assert.IsNotNull(result);
		Assert.Contains("cachedsystem", result.SystemCodes);
		Assert.Contains("cachedclass", result.Classes);
		Assert.IsFalse(result.SystemCodes.Contains("newsystem"));
		Assert.IsFalse(result.Classes.Contains("newclass"));
		Assert.AreEqual(1, _testCache.Count());
	}

	[TestMethod]
	public async Task GetTokens_WithEmptyDatabase_ReturnsEmptyCollections()
	{
		var result = await _movieSearchTokens.GetTokens();

		Assert.IsNotNull(result);
		Assert.IsEmpty(result.SystemCodes);
		Assert.IsEmpty(result.Classes);
		Assert.IsEmpty(result.Tags);
		Assert.IsEmpty(result.Genres);
		Assert.IsEmpty(result.Flags);
		Assert.IsTrue(_testCache.ContainsKey(CacheKeys.MovieTokens));
	}

	[TestMethod]
	public async Task GetTokens_WithMixedCaseData_ConvertsToLowercase()
	{
		_db.Add(new GameSystem { Id = 1, Code = "MixedCaseSystem" });
		_db.Add(new PublicationClass { Id = 1, Name = "MixedCaseClass" });
		_db.Add(new Tag { Id = 1, Code = "MixedCaseTag" });
		_db.Add(new Genre { Id = 1, DisplayName = "Mixed Case Genre" });
		_db.Add(new Flag { Id = 1, Name = "Mixed Case Flag", Token = "Mixed-Case-Token" });
		await _db.SaveChangesAsync();

		var result = await _movieSearchTokens.GetTokens();

		// All values should be lowercase
		Assert.Contains("mixedcasesystem", result.SystemCodes);
		Assert.Contains("mixedcaseclass", result.Classes);
		Assert.Contains("mixedcasetag", result.Tags);
		Assert.Contains("mixed case genre", result.Genres);
		Assert.Contains("mixed-case-token", result.Flags);

		// Verify no uppercase versions exist
		Assert.IsFalse(result.SystemCodes.Contains("MixedCaseSystem"));
		Assert.IsFalse(result.Classes.Contains("MixedCaseClass"));
		Assert.IsFalse(result.Tags.Contains("MixedCaseTag"));
		Assert.IsFalse(result.Genres.Contains("Mixed Case Genre"));
		Assert.IsFalse(result.Flags.Contains("Mixed-Case-Token"));
	}

	[TestMethod]
	public async Task GetTokens_WithMultipleItems_ReturnsAllItems()
	{
		const int itemCount = 5;
		for (var i = 1; i <= itemCount; i++)
		{
			_db.Add(new GameSystem { Id = i, Code = $"System{i}" });
			_db.Add(new PublicationClass { Id = i, Name = $"Class{i}" });
			_db.Add(new Tag { Id = i, Code = $"Tag{i}" });
			_db.Add(new Genre { Id = i, DisplayName = $"Genre{i}" });
			_db.Add(new Flag { Id = i, Name = $"Flag{i}", Token = $"token{i}" });
		}

		await _db.SaveChangesAsync();
		await _db.SaveChangesAsync();

		var result = await _movieSearchTokens.GetTokens();

		Assert.AreEqual(itemCount, result.SystemCodes.Count);
		Assert.AreEqual(itemCount, result.Classes.Count);
		Assert.AreEqual(itemCount, result.Tags.Count);
		Assert.AreEqual(itemCount, result.Genres.Count);
		Assert.AreEqual(itemCount, result.Flags.Count);

		Assert.Contains("system1", result.SystemCodes);
		Assert.Contains("system5", result.SystemCodes);
		Assert.Contains("class1", result.Classes);
		Assert.Contains("class5", result.Classes);
		Assert.Contains("tag1", result.Tags);
		Assert.Contains("tag5", result.Tags);
		Assert.Contains("genre1", result.Genres);
		Assert.Contains("genre5", result.Genres);
		Assert.Contains("token1", result.Flags);
		Assert.Contains("token5", result.Flags);
	}

	[TestMethod]
	public void MovieTokens_DefaultProperties_AreInitializedCorrectly()
	{
		var movieTokens = new MovieTokens();

		// Verify years range (2000 to current year + 1)
		var currentYear = DateTime.UtcNow.Year;
		const int expectedMinYear = 2000;
		var expectedMaxYear = currentYear + 1;
		Assert.Contains(expectedMinYear, movieTokens.Years);
		Assert.Contains(currentYear, movieTokens.Years);
		Assert.Contains(expectedMaxYear, movieTokens.Years);
		Assert.AreEqual(expectedMaxYear - expectedMinYear + 1, movieTokens.Years.Count);

		// Verify default boolean values
		Assert.IsFalse(movieTokens.ShowObsoleted);
		Assert.IsFalse(movieTokens.OnlyObsoleted);
		Assert.IsNull(movieTokens.Limit);
	}
}
