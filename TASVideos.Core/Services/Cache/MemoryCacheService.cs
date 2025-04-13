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

	public void Set<T>(string key, T data, TimeSpan? cacheTime = null)
	{
		cache.Set(key, data, cacheTime ?? settings.CacheSettings.CacheDuration);
	}
}
