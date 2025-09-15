using Microsoft.Extensions.Caching.Memory;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class MemoryCacheServiceTests : TestDbBase
{
	private readonly IMemoryCache _memoryCache;
	private readonly MemoryCacheService _memoryCacheService;

	public MemoryCacheServiceTests()
	{
		_memoryCache = Substitute.For<IMemoryCache>();
		var appSettings = new AppSettings
		{
			CacheSettings = new AppSettings.CacheSetting
			{
				CacheDurationInSeconds = 300
			}
		};

		_memoryCacheService = new MemoryCacheService(_memoryCache, appSettings);
	}

	[TestMethod]
	public void TryGetValue_WhenValueExists_ReturnsTrue()
	{
		const string key = "test-key";
		const string expectedValue = "test-value";
		_memoryCache.TryGetValue(key, out Arg.Any<object>()!)
			.Returns(x =>
			{
				x[1] = expectedValue;
				return true;
			});

		var result = _memoryCacheService.TryGetValue<string>(key, out var actualValue);

		Assert.IsTrue(result);
		Assert.AreEqual(expectedValue, actualValue);
	}

	[TestMethod]
	public void TryGetValue_WhenValueDoesNotExist_ReturnsFalse()
	{
		const string key = "non-existent-key";
		_memoryCache.TryGetValue(key, out Arg.Any<object>()!)
			.Returns(false);

		var result = _memoryCacheService.TryGetValue<string>(key, out var actualValue);

		Assert.IsFalse(result);
		Assert.IsNull(actualValue);
	}

	[TestMethod]
	public void TryGetValue_WithComplexType_ReturnsCorrectObject()
	{
		const string key = "complex-key";
		var expectedValue = new TestObject { Id = 123, Name = "Test" };
		_memoryCache.TryGetValue(key, out Arg.Any<object>()!)
			.Returns(x =>
			{
				x[1] = expectedValue;
				return true;
			});

		var result = _memoryCacheService.TryGetValue<TestObject>(key, out var actualValue);

		Assert.IsTrue(result);
		Assert.AreEqual(expectedValue.Id, actualValue.Id);
		Assert.AreEqual(expectedValue.Name, actualValue.Name);
	}

	[TestMethod]
	public void Set_WithDefaultCacheTime_CreatesEntryWithDefaultExpiration()
	{
		const string key = "test-key";
		const string value = "test-value";
		var expectedDuration = TimeSpan.FromSeconds(300);

		var mockEntry = Substitute.For<ICacheEntry>();
		_memoryCache.CreateEntry(key).Returns(mockEntry);

		_memoryCacheService.Set(key, value);

		_memoryCache.Received(1).CreateEntry(key);
		Assert.AreEqual(value, mockEntry.Value);
		Assert.AreEqual(expectedDuration, mockEntry.AbsoluteExpirationRelativeToNow);
	}

	[TestMethod]
	public void Set_WithCustomCacheTime_CreatesEntryWithProvidedDuration()
	{
		const string key = "test-key";
		const string value = "test-value";
		var customDuration = TimeSpan.FromMinutes(10);

		var mockEntry = Substitute.For<ICacheEntry>();
		_memoryCache.CreateEntry(key).Returns(mockEntry);

		_memoryCacheService.Set(key, value, customDuration);

		_memoryCache.Received(1).CreateEntry(key);
		Assert.AreEqual(value, mockEntry.Value);
		Assert.AreEqual(customDuration, mockEntry.AbsoluteExpirationRelativeToNow);
	}

	[TestMethod]
	public void Set_WithNullCacheTime_UsesAppSettingsDuration()
	{
		const string key = "test-key";
		const string value = "test-value";
		var expectedDuration = TimeSpan.FromSeconds(300);

		var mockEntry = Substitute.For<ICacheEntry>();
		_memoryCache.CreateEntry(key).Returns(mockEntry);

		_memoryCacheService.Set(key, value);

		_memoryCache.Received(1).CreateEntry(key);
		Assert.AreEqual(value, mockEntry.Value);
		Assert.AreEqual(expectedDuration, mockEntry.AbsoluteExpirationRelativeToNow);
	}

	[TestMethod]
	public void Set_WithComplexObject_StoresCorrectly()
	{
		const string key = "complex-key";
		var value = new TestObject { Id = 456, Name = "Complex Test" };
		var customDuration = TimeSpan.FromHours(1);

		var mockEntry = Substitute.For<ICacheEntry>();
		_memoryCache.CreateEntry(key).Returns(mockEntry);

		_memoryCacheService.Set(key, value, customDuration);

		_memoryCache.Received(1).CreateEntry(key);
		Assert.AreEqual(value, mockEntry.Value);
		Assert.AreEqual(customDuration, mockEntry.AbsoluteExpirationRelativeToNow);
	}

	[TestMethod]
	public void Remove_WithValidKey_CallsMemoryCacheRemove()
	{
		const string key = "key-to-remove";

		_memoryCacheService.Remove(key);

		_memoryCache.Received(1).Remove(key);
	}

	[TestMethod]
	public void TryGetValue_WithValidData_ReturnsCorrectType()
	{
		const string intKey = "int-key";
		const int intValue = 42;
		_memoryCache.TryGetValue(intKey, out Arg.Any<object>()!)
			.Returns(x =>
			{
				x[1] = intValue;
				return true;
			});

		var intResult = _memoryCacheService.TryGetValue<int>(intKey, out var actualIntValue);
		Assert.IsTrue(intResult);
		Assert.AreEqual(intValue, actualIntValue);
	}

	private class TestObject
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
	}
}
