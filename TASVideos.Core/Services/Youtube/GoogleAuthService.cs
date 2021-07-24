using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
			
			if (!response.IsSuccessStatusCode)
			{
				var errorResponse = await response.Content.ReadAsStringAsync();
				_logger.LogError("Unable to authorize google apis: " + errorResponse);
				return "";
			}

			var tokenResponse = await response.ReadAsync<AccessTokenResponse>();

			// Subtract a bit of time to ensure it does not expire between the time of accessing and using it
			_cache.Set(CacheKey, tokenResponse.AccessToken, tokenResponse.ExpiresAt - 10);

			return tokenResponse.AccessToken;
		}
	}
}
