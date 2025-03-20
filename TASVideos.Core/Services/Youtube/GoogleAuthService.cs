using Microsoft.Extensions.Logging;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Youtube;

public interface IGoogleAuthService
{
	bool IsYoutubeEnabled();
	Task<string> GetYoutubeAccessToken();
}

internal class GoogleAuthService(
	IHttpClientFactory httpClientFactory,
	ICacheService cache,
	AppSettings settings,
	ILogger<GoogleAuthService> logger)
	: IGoogleAuthService
{
	private const string YoutubeCacheKey = "GoogleAuthAccessTokenCacheForYoutube";
	private readonly HttpClient _client = httpClientFactory.CreateClient(HttpClients.GoogleAuth)
		?? throw new InvalidOperationException($"Unable to initalize {HttpClients.GoogleAuth} client");

	public bool IsYoutubeEnabled() => settings.YouTube.IsEnabled();

	public async Task<string> GetYoutubeAccessToken() => await GetAccessToken(YoutubeCacheKey);

	private async Task<string> GetAccessToken(string cacheKey)
	{
		if (cache.TryGetValue(cacheKey, out string accessToken))
		{
			return accessToken;
		}

		var body = new AccessTokenRequest
		{
			ClientId = settings.YouTube.ClientId,
			ClientSecret = settings.YouTube.ClientSecret,
			RefreshToken = settings.YouTube.RefreshToken
		}.ToStringContent();

		var response = await _client.PostAsync("token", body);

		if (!response.IsSuccessStatusCode)
		{
			var errorResponse = await response.Content.ReadAsStringAsync();
			logger.LogError(
				"Unable to authorize google apis for clientId: {clientId}: {errorResponse}",
				settings.YouTube.ClientId,
				errorResponse);
			return "";
		}

		var tokenResponse = await response.ReadAsync<AccessTokenResponse>();

		if (tokenResponse.ExpiresAt > 10)
		{
			// Subtract a bit of time to ensure it does not expire between the time of accessing and using it
			cache.Set(cacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(tokenResponse.ExpiresAt - 10));
		}

		return tokenResponse.AccessToken;
	}
}
