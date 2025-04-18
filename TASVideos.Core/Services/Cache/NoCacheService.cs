﻿namespace TASVideos.Core.Services;

public class NoCacheService : ICacheService
{
	public bool TryGetValue<T>(string key, out T value)
	{
		value = default!;
		return false;
	}

	public void Set(string key, object? data, TimeSpan? cacheTime = null)
	{
	}

	public void Remove(string key)
	{
	}
}
