namespace TASVideos.Core.Services;

public static class CacheServiceExtensions
{
	/// <summary>
	/// Returns a dictionary of all cached values for the given cache keys
	/// Only entries that have a cached value are returned
	/// </summary>
	public static IDictionary<string, T> GetAll<T>(this ICacheService cache, IEnumerable<string> keys)
	{
		var dic = new Dictionary<string, T>();
		foreach (var key in keys)
		{
			if (cache.TryGetValue(key, out T value))
			{
				dic.Add(key, value);
			}
		}

		return dic;
	}
}
