using System;
using Microsoft.Extensions.Caching.Memory;

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
		// TODO: pass settings in
		public const int CacheDurationInSeconds = 60;

		private readonly IMemoryCache _cache;

		public MemoryCacheService(IMemoryCache cache)
		{
			_cache = cache;
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
				_cache.Set(key, data, new TimeSpan(0, 0, cacheTime ?? CacheDurationInSeconds));
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
