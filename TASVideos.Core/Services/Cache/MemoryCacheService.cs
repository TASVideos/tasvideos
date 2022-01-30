using Microsoft.Extensions.Caching.Memory;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services;

public class MemoryCacheService : ICacheService
{
	private readonly IMemoryCache _cache;
	private readonly AppSettings _settings;

	public MemoryCacheService(IMemoryCache cache, AppSettings settings)
	{
		_cache = cache;
		_settings = settings;
	}

	public bool TryGetValue<T>(string key, out T value)
	{
		return _cache.TryGetValue(key, out value);
	}

	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	public void Set(string key, object? data, int? cacheTime)
	{
		using var entry = _cache.CreateEntry(key);
		entry.Value = data;
		_cache.Set(key, data, new TimeSpan(0, 0, cacheTime ?? _settings.CacheSettings.CacheDurationInSeconds));
	}
}
