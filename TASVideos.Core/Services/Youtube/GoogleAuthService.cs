using Microsoft.Extensions.Logging;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Youtube;

public interface IGoogleAuthService
{
	bool IsYoutubeEnabled();
	Task<string> GetYoutubeAccessToken();

	bool IsGmailEnabled();
	Task<string> GetGmailAccessToken();
}

internal class GoogleAuthService : IGoogleAuthService
{
	private const string YoutubeCacheKey = "GoogleAuthAccessTokenCacheForYoutube";
	private const string GmailCacheKey = "GoogleAuthAccessTokenCacheForGmail";
	private readonly HttpClient _client;
	private readonly ICacheService _cache;
	private readonly AppSettings _settings;
	private readonly ILogger<GoogleAuthService> _logger;

	public GoogleAuthService(
		IHttpClientFactory httpClientFactory,
		ICacheService cache,
		AppSettings settings,
		ILogger<GoogleAuthService> logger)
	{
		_client = httpClientFactory.CreateClient(HttpClients.GoogleAuth)
			?? throw new InvalidOperationException($"Unable to initalize {HttpClients.GoogleAuth} client");
		_cache = cache;
		_settings = settings;
		_logger = logger;
	}

	public bool IsYoutubeEnabled() => _settings.YouTube.IsEnabled();

	public async Task<string> GetYoutubeAccessToken() => await GetAccessToken(_settings.YouTube, YoutubeCacheKey);

	public bool IsGmailEnabled() => _settings.Gmail.IsEnabled();

	public async Task<string> GetGmailAccessToken() => await GetAccessToken(_settings.Gmail, GmailCacheKey);

	private async Task<string> GetAccessToken(AppSettings.GoogleAuthSettings settings, string cacheKey)
	{
		if (_cache.TryGetValue(cacheKey, out string accessToken))
		{
			return accessToken;
		}

		var body = new AccessTokenRequest
		{
			ClientId = settings.ClientId,
			ClientSecret = settings.ClientSecret,
			RefreshToken = settings.RefreshToken
		}.ToStringContent();

		var response = await _client.PostAsync("token", body);

		if (!response.IsSuccessStatusCode)
		{
			var errorResponse = await response.Content.ReadAsStringAsync();
			_logger.LogError(
				"Unable to authorize google apis for clientId: {clientId}: {errorResponse}",
				settings.ClientId,
				errorResponse);
			return "";
		}

		var tokenResponse = await response.ReadAsync<AccessTokenResponse>();

		if (tokenResponse.ExpiresAt > 10)
		{
			// Subtract a bit of time to ensure it does not expire between the time of accessing and using it
			_cache.Set(cacheKey, tokenResponse.AccessToken, tokenResponse.ExpiresAt - 10);
		}

		return tokenResponse.AccessToken;
	}
}
