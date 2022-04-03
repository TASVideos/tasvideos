using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Cache;

public class RedisCacheService : ICacheService
{
	private readonly bool _enabled;
	private readonly ILogger<RedisCacheService> _logger;
	private static IDatabase _cache = null!;
	private static Lazy<ConnectionMultiplexer>? _connection;
	private readonly int _cacheDurationInSeconds;
	private static readonly JsonSerializerOptions SerializerSettings = new()
	{
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};

	public RedisCacheService(AppSettings settings, ILogger<RedisCacheService> logger)
	{
		_logger = logger;

		if (string.IsNullOrWhiteSpace(settings.CacheSettings.ConnectionString))
		{
			_logger.LogWarning("Redis connection string not set, skipping Redis initialization");
			_enabled = false;
			return;
		}

		try
		{
			_cacheDurationInSeconds = settings.CacheSettings.CacheDurationInSeconds;
			_connection ??= new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(settings.CacheSettings.ConnectionString));
			var redis = _connection.Value;
			_cache = redis.GetDatabase();
			_enabled = true;
		}
		catch (RedisConnectionException)
		{
			_enabled = false;
		}
	}

	public bool TryGetValue<T>(string key, out T value)
	{
		if (!_enabled)
		{
			value = default!;
			return false;
		}

		try
		{
			RedisValue data = _cache.StringGet(key);
			if (data.IsNullOrEmpty)
			{
				value = default!;
				return false;
			}

			value = JsonSerializer.Deserialize<T>(data) ?? default!;
			return true;
		}
		catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException)
		{
			_logger.LogWarning("Redis failure on key {key}, silently falling back to uncached", key);
			value = default!;
			return false;
		}
	}

	public void Set(string key, object? data, int? cacheTime = null)
	{
		if (_enabled)
		{
			return;
		}

		try
		{
			var serializedData = JsonSerializer.Serialize(data, SerializerSettings);
			var timeout = TimeSpan.FromSeconds(cacheTime ?? _cacheDurationInSeconds);
			_cache.StringSet(key, serializedData, timeout);
		}
		catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException)
		{
			_logger.LogWarning("Redis failure on setting key {key}, silently falling back and not adding to cache", key);
		}
	}

	public void Remove(string key)
	{
		if (_enabled)
		{
			return;
		}

		_cache.KeyDelete(key);
	}
}
