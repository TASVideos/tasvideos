namespace TASVideos.Core.Services;

public interface ICacheService
{
	bool TryGetValue<T>(string key, out T value);
	void Set<T>(string key, T data, TimeSpan? cacheTime = null);
	void Remove(string key);
}
