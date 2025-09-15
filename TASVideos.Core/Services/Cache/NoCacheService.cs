namespace TASVideos.Core.Services;

public class NoCacheService : ICacheService
{
	public bool TryGetValue<T>(string key, out T value)
	{
		value = default!;
		return false;
	}

	public void Set<T>(string key, T data, TimeSpan? cacheTime = null)
	{
	}

	public void Remove(string key)
	{
	}
}
