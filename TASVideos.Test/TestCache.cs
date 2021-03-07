using System.Collections.Generic;
using TASVideos.Services;

namespace TASVideos.Test
{
	/// <summary>
	/// Provides a dummy cache service that has helpful methods for unit testing
	/// </summary>
	public class TestCache : ICacheService
	{
		private readonly Dictionary<string, object?> _cache = new ();

		public bool TryGetValue<T>(string key, out T value)
		{
			var result = _cache.TryGetValue(key, out object? cached);
			value = (T)cached!;
			return result;
		}

		public void Set(string key, object? data, int? cacheTime = null)
		{
			_cache[key] = data;
		}

		public void Remove(string key) => _cache.Remove(key);

		public int Count() => _cache.Count;

		public bool ContainsKey(string key) => _cache.ContainsKey(key);
	}
}
