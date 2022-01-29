namespace TASVideos.Core.Services;

public interface ICacheService
{
	bool TryGetValue<T>(string key, out T value);
	void Set(string key, object? data, int? cacheTime = null);
	void Remove(string key);
}
