using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
		ICacheService redisCache,
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<TwitterDistributorV2> logger)
	{
		_redisCacheService = redisCache;
		_settings = appSettings.TwitterV2;
		_twitterClient = httpClientFactory.CreateClient(HttpClients.TwitterV2);
		_accessTokenClient = httpClientFactory.CreateClient(HttpClients.TwitterAuth);
		_logger = logger;

		RefreshTokens().GetAwaiter();

		_twitterClient.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Bearer",
				AccessToken);

		_accessTokenClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public async Task Post(IPostable post)
	{
		if (!_settings.IsEnabled())
		{
			return;
		}

		await RefreshTokens();

		HttpRequestMessage tweet = new HttpRequestMessage();
		tweet.Method = HttpMethod.Post;

		var tweetData = new List<KeyValuePair<string, string>>()
		{
			new KeyValuePair<string, string> ("text", post.Body)
		};

		tweet.Content = new FormUrlEncodedContent(tweetData);

		var response = await _twitterClient.SendAsync(tweet);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError($"Error sending tweet: {response.ReasonPhrase}");
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

				CacheValues();
			}
		}
	}

	public void RetrieveCachedValues()
	{
		var keys = _redisCacheService.GetAll<string>(new List<string>() { TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY, TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY });

		_refreshToken = keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_KEY];
		_nextRefreshTime = DateTime.Parse(keys[TwitterDistributorConstants.TWITTER_REFRESH_TOKEN_TIME_KEY]);
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
		formData.Add(new KeyValuePair<string, string>("scope", "offline.access tweet.write"));

		string basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

		_accessTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthHeader);

		var response = await _accessTokenClient.PostAsync("", new FormUrlEncodedContent(formData));

		if (response.IsSuccessStatusCode)
		{
			var responseData = JsonSerializer.Deserialize<TwitterRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			_accessToken = responseData!.AccessToken;
			_refreshToken = responseData!.RefreshToken;
			_nextRefreshTime = DateTime.UtcNow.AddSeconds(refreshTokenDuration);
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
