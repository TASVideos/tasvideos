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
		Assert.IsTrue(result.SystemCodes.Contains("testsystem"));
		Assert.IsTrue(result.Classes.Contains("testclass"));
		Assert.IsTrue(result.Tags.Contains("testtag"));
		Assert.IsTrue(result.Genres.Contains("testgenre"));
		Assert.IsTrue(result.Flags.Contains("test-token"));

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
		Assert.IsTrue(result.SystemCodes.Contains("cachedsystem"));
		Assert.IsTrue(result.Classes.Contains("cachedclass"));
		Assert.IsFalse(result.SystemCodes.Contains("newsystem"));
		Assert.IsFalse(result.Classes.Contains("newclass"));
		Assert.AreEqual(1, _testCache.Count());
	}

	[TestMethod]
	public async Task GetTokens_WithEmptyDatabase_ReturnsEmptyCollections()
	{
		var result = await _movieSearchTokens.GetTokens();

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.SystemCodes.Count);
		Assert.AreEqual(0, result.Classes.Count);
		Assert.AreEqual(0, result.Tags.Count);
		Assert.AreEqual(0, result.Genres.Count);
		Assert.AreEqual(0, result.Flags.Count);
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
		Assert.IsTrue(result.SystemCodes.Contains("mixedcasesystem"));
		Assert.IsTrue(result.Classes.Contains("mixedcaseclass"));
		Assert.IsTrue(result.Tags.Contains("mixedcasetag"));
		Assert.IsTrue(result.Genres.Contains("mixed case genre"));
		Assert.IsTrue(result.Flags.Contains("mixed-case-token"));

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

		Assert.IsTrue(result.SystemCodes.Contains("system1"));
		Assert.IsTrue(result.SystemCodes.Contains("system5"));
		Assert.IsTrue(result.Classes.Contains("class1"));
		Assert.IsTrue(result.Classes.Contains("class5"));
		Assert.IsTrue(result.Tags.Contains("tag1"));
		Assert.IsTrue(result.Tags.Contains("tag5"));
		Assert.IsTrue(result.Genres.Contains("genre1"));
		Assert.IsTrue(result.Genres.Contains("genre5"));
		Assert.IsTrue(result.Flags.Contains("token1"));
		Assert.IsTrue(result.Flags.Contains("token5"));
	}

	[TestMethod]
	public void MovieTokens_DefaultProperties_AreInitializedCorrectly()
	{
		var movieTokens = new MovieTokens();

		// Verify years range (2000 to current year + 1)
		var currentYear = DateTime.UtcNow.Year;
		const int expectedMinYear = 2000;
		var expectedMaxYear = currentYear + 1;
		Assert.IsTrue(movieTokens.Years.Contains(expectedMinYear));
		Assert.IsTrue(movieTokens.Years.Contains(currentYear));
		Assert.IsTrue(movieTokens.Years.Contains(expectedMaxYear));
		Assert.AreEqual(expectedMaxYear - expectedMinYear + 1, movieTokens.Years.Count);

		// Verify default boolean values
		Assert.IsFalse(movieTokens.ShowObsoleted);
		Assert.IsFalse(movieTokens.OnlyObsoleted);
		Assert.IsNull(movieTokens.Limit);
	}
}
