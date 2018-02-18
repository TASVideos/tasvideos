using System;
using Microsoft.Extensions.Caching.Memory;

namespace TASVideos.Services
{
	public interface ICacheService
	{
		T Get<T>(string key);
		void Set(string key, object data, int? cacheTime = null);
		bool IsSet(string key);
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

		public T Get<T>(string key)
		{
			_cache.TryGetValue(key, out var obj);
			return (T)obj;
		}

		public bool IsSet(string key)
		{
			return _cache.TryGetValue(key, out _);
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
		public T Get<T>(string key)
		{
			return default(T);
		}

		public bool IsSet(string key)
		{
			return false;
		}

		public void Remove(string key)
		{
		}

		public void Set(string key, object data, int? cacheTime)
		{
		}
	}
}
