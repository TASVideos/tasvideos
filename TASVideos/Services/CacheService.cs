using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace TASVideos.Services
{
	public interface ICacheService
	{
		bool TryGetValue<T>(string key, out T value);
		void Set(string key, object data, int? cacheTime = null);
		void Remove(string key);
	}

	public class MemoryCacheService : ICacheService
	{
		private readonly IMemoryCache _cache;
		private readonly IOptions<AppSettings> _settings;

		public MemoryCacheService(IMemoryCache cache, IOptions<AppSettings> settings)
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

		public void Set(string key, object data, int? cacheTime)
		{
			using (var entry = _cache.CreateEntry(key))
			{
				entry.Value = data;
				_cache.Set(key, data, new TimeSpan(0, 0, cacheTime ?? _settings.Value.CacheSettings.CacheDurationInSeconds));
			}
		}
	}

	public class NoCacheService : ICacheService
	{
		public bool TryGetValue<T>(string key, out T value)
		{
			value = default(T);
			return false;
		}

		public void Set(string key, object data, int? cacheTime)
		{
		}

		public void Remove(string key)
		{
		}
	}
}
