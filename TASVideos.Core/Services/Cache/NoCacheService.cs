using System.Diagnostics.CodeAnalysis;

namespace TASVideos.Core.Services;

public class NoCacheService : ICacheService
{
	[RequiresUnreferencedCode(nameof(ICacheService.TryGetValue))]
	public bool TryGetValue<T>(string key, out T value)
	{
		value = default!;
		return false;
	}

	[RequiresUnreferencedCode(nameof(ICacheService.Set))]
	public void Set(string key, object? data, int? cacheTime)
	{
	}

	public void Remove(string key)
	{
	}
}
