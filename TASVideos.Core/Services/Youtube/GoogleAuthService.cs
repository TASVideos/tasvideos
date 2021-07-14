using System;
using System.Net.Http;
using System.Threading.Tasks;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Youtube
{
	public interface IGoogleAuthService
	{
		bool IsEnabled();
		Task<string> GetAccessToken();
	}

	internal class GoogleAuthService : IGoogleAuthService
	{
		private const string CacheKey = "GoogleAuthAccessTokenCache";
		private readonly HttpClient _client;
		private readonly ICacheService _cache;
		private readonly AppSettings _settings;

		public GoogleAuthService(
			IHttpClientFactory httpClientFactory,
			ICacheService cache,
			AppSettings settings)
		{
			_client = httpClientFactory.CreateClient(HttpClients.GoogleAuth)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.GoogleAuth} client");
			_cache = cache;
			_settings = settings;
		}

		public bool IsEnabled()
		{
			return !string.IsNullOrWhiteSpace(_settings.YouTube.RefreshToken)
				&& !string.IsNullOrWhiteSpace(_settings.YouTube.ClientId)
				&& !string.IsNullOrWhiteSpace(_settings.YouTube.ClientSecret);
		}

		public async Task<string> GetAccessToken()
		{
			if (_cache.TryGetValue(CacheKey, out string accessToken))
			{
				return accessToken;
			}

			var body = new AccessTokenRequest
			{
				ClientId = _settings.YouTube.ClientId,
				ClientSecret = _settings.YouTube.ClientSecret,
				RefreshToken = _settings.YouTube.RefreshToken
			}.ToStringContent();

			var response = await _client.PostAsync("token", body);
			response.EnsureSuccessStatusCode();
			var tokenResponse = await response.ReadAsync<AccessTokenResponse>();

			// Subtract a bit of time to ensure it does not expire between the time of accessing and using it
			_cache.Set(CacheKey, tokenResponse.AccessToken, tokenResponse.ExpiresAt - 10);

			return tokenResponse.AccessToken;
		}
	}
}
