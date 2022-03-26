using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Cache;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class TwitterDistributorV2 : IPostDistributor
{
	private readonly HttpClient _twitterClient;
	private readonly HttpClient _accessTokenClient;
	private readonly AppSettings.TwitterConnectionV2 _settings;
	private readonly ILogger<TwitterDistributorV2> _logger;
	private readonly ICacheService _redisCacheService;

	private string? AccessToken { 
		get
		{
			return _accessToken;
		}
		set
		{
			_accessToken = value;
		}
	}

	private string? _accessToken;
	private string? _refreshToken;
	private DateTime? _nextRefreshTime;

	private const int refreshTokenDuration = 2 * 60 * 60 - 30;	// Two hours minus thirty seconds in seconds.  How long the retrieved access token will last.

	public TwitterDistributorV2 (
		RedisCacheService redisCache,		// Intentionally using Redis Cache here, if we need to turn Redis off, come up with a new solution.  -- Invariel, March 2022.
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<TwitterDistributorV2> logger)
	{
		_redisCacheService = redisCache;
		_settings = appSettings.TwitterV2;
		_twitterClient = httpClientFactory.CreateClient(HttpClients.TwitterV2);
		_accessTokenClient = httpClientFactory.CreateClient(HttpClients.TwitterAuth);
		_logger = logger;
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public async Task Post(IPostable post)
	{
		if (!_settings.IsEnabled())
		{
			return;
		}

		await RefreshTokens();
		_twitterClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Bearer",
				AccessToken);

		var tweetData = new
		{
			text = post.Body
		};

		var response = await _twitterClient.PostAsync("", tweetData.ToStringContent());

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("Error sending tweet: {reasonPhrase}", response.ReasonPhrase);
		}
	}

	public async Task RefreshTokens()
	{
		if (_nextRefreshTime == null || DateTime.UtcNow > _nextRefreshTime)
		{
			RetrieveCachedValues();

			if (DateTime.UtcNow > _nextRefreshTime || _accessToken == null)
			{
				await RequestTokensFromTwitter();
			}
		}
	}

	public void RetrieveCachedValues()
	{
		var keys = _redisCacheService.GetAll<string>(new List<string>
		{
			TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY,
			TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY
		});

		if (!keys.ContainsKey(TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY)
			|| string.IsNullOrWhiteSpace(keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY]))
		{
			_logger.LogError("Unable to initialize twitter, missing value {token}", TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY);
			return;
		}

		_refreshToken = keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY];
		_nextRefreshTime = DateTime.UtcNow.AddDays(-1);
		_logger.LogError("Refresh token {token}", _refreshToken);
		if (keys.ContainsKey(TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY)
			&& string.IsNullOrWhiteSpace(keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY]))
		{
			var result = DateTime.TryParse(keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY], out var time);
			if (result)
			{
				_nextRefreshTime = time;
			}
		}
	}

	public void CacheValues()
	{
		_redisCacheService.Set(TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY, _refreshToken);
		_redisCacheService.Set(TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY, _nextRefreshTime.ToString());
	}

	public async Task RequestTokensFromTwitter()
	{
		var formData = new List<KeyValuePair<string, string>>();
		formData.Add(new KeyValuePair<string, string>("refresh_token", _refreshToken!));
		formData.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
		formData.Add(new KeyValuePair<string, string>("scope", "offline.access tweet.read tweet.write users.read"));

		string basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

		_accessTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthHeader);

		var response = await _accessTokenClient.PostAsync("", new FormUrlEncodedContent(formData));

		if (response.IsSuccessStatusCode)
		{
			var responseData = JsonSerializer.Deserialize<TwitterRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			_accessToken = responseData!.AccessToken;
			_refreshToken = responseData!.RefreshToken;
			_nextRefreshTime = DateTime.UtcNow.AddSeconds(refreshTokenDuration);
			CacheValues();
		}
	}
}

public class TwitterDistributorConstants
{
	public static string TWITTER_REFRESH_TOKEN_KEY = "TwitterRefreshToken";
	public static string TWITTER_REFRESH_TOKEN_TIME_KEY = "TwitterRefreshTokenTime";
}

public class TwitterRefreshTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";
	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = "";
}
