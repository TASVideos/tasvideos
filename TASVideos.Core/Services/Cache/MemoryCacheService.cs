using Microsoft.Extensions.Caching.Memory;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services;

public class MemoryCacheService(IMemoryCache cache, AppSettings settings) : ICacheService
{
	public bool TryGetValue<T>(string key, out T value)
	{
		return cache.TryGetValue(key, out value!);
	}

	public void Remove(string key)
	{
		cache.Remove(key);
	}

	public void Set(string key, object? data, int? cacheTime)
	{
		cache.Set(key, data, new TimeSpan(0, 0, cacheTime ?? settings.CacheSettings.CacheDurationInSeconds));
	}
}
