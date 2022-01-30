using Newtonsoft.Json;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests;

internal class WikiTestCache : ICacheService
{
	private static readonly JsonSerializerSettings SerializerSettings = new()
	{
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore
	};

	private readonly Dictionary<string, string> _cache = new();

	public void AddPage(WikiPage page)
	{
		Set($"{CacheKeys.CurrentWikiCache}-{page.PageName.ToLower()}", page);
	}

	public List<WikiPage> PageCache()
	{
		List<WikiPage> pages = _cache
				.Select(kvp => JsonConvert.DeserializeObject<WikiPage>(kvp.Value)!)
				.ToList();

		return pages;
	}

	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	public void Set(string key, object? data, int? cacheTime = null)
	{
		if (data is not WikiPage)
		{
			throw new InvalidOperationException($"data must be of type {nameof(WikiPage)}");
		}

		var serialized = JsonConvert.SerializeObject(data, SerializerSettings);
		_cache[key] = serialized;
	}

	public bool TryGetValue<T>(string key, out T value)
	{
		var result = _cache.TryGetValue(key, out string? cached);
		value = result
			? JsonConvert.DeserializeObject<T>(cached ?? "")!
			: default!;

		return result;
	}
}
