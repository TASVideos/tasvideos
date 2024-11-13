using System.Diagnostics.CodeAnalysis;

using TASVideos.Core.Services.Cache;

namespace TASVideos.Core.Services;

public interface ICacheService
{
	[RequiresUnreferencedCode(nameof(RedisCacheService.TryGetValue))]
	bool TryGetValue<T>(string key, out T value);
	[RequiresUnreferencedCode(nameof(RedisCacheService.Set))]
	void Set(string key, object? data, int? cacheTime = null);
	void Remove(string key);
}
