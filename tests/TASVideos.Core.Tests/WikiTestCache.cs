using System.Text.Json;
using System.Text.Json.Serialization;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests;

internal class WikiTestCache : ICacheService
{
	private static readonly JsonSerializerOptions SerializerSettings = new()
	{
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};

	private readonly Dictionary<string, string> _cache = [];

	public void AddPage(WikiPage page)
	{
		Set($"{CacheKeys.CurrentWikiCache}-{page.PageName.ToLower()}", page.ToWikiResult());
	}

	public List<WikiResult> PageCache()
	{
		return _cache
			.Select(kvp => JsonSerializer.Deserialize<WikiResult>(kvp.Value)!)
			.ToList();
	}

	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	public void Set<T>(string key, T data, TimeSpan? cacheTime = null)
	{
		if (data is not WikiResult)
		{
			throw new InvalidOperationException($"data must be of type {nameof(WikiResult)}");
		}

		var serialized = JsonSerializer.Serialize(data, SerializerSettings);
		_cache[key] = serialized;
	}

	public bool TryGetValue<T>(string key, out T value)
	{
		var result = _cache.TryGetValue(key, out string? cached);
		value = result
			? JsonSerializer.Deserialize<T>(cached ?? "")!
			: default!;

		return result;
	}
}
