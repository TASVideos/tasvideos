using Newtonsoft.Json;
using TASVideos.Core.Services;

namespace TASVideos.Tests.Base;

/// <summary>
/// Provides a dummy cache service that has helpful methods for unit testing
/// </summary>
public class TestCache : ICacheService
{
	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	};

	private readonly Dictionary<string, string> _cache = new();

	public bool TryGetValue<T>(string key, out T value)
	{
		var result = _cache.TryGetValue(key, out string? cached);
		value = result
			? JsonConvert.DeserializeObject<T>(cached ?? "")!
			: default!;

		return result;
	}

	public void Set(string key, object? data, int? cacheTime = null)
	{
		var str = JsonConvert.SerializeObject(data, SerializerSettings);
		_cache[key] = str;
	}

	public void Remove(string key) => _cache.Remove(key);

	public int Count() => _cache.Count;

	public bool ContainsKey(string key) => _cache.ContainsKey(key);
}
