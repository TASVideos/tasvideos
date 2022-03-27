using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MimeKit;
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

	private string? _accessToken;
	private string? _refreshToken;
	private DateTime? _nextRefreshTime;

	private const int RefreshTokenDuration = 2 * 60 * 60 - 30;	// Two hours minus thirty seconds in seconds.  How long the retrieved access token will last.

	public TwitterDistributorV2 (
		// Intentionally using Redis Cache here, if we need to turn Redis off, come up with a new solution.  -- Invariel, March 2022.
		RedisCacheService redisCache,
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
				_accessToken);

		var tweetData = new
		{
			text = GenerateTwitterMessage(post)
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
			TwitterDistributorConstants.RefreshToken,
			TwitterDistributorConstants.RefreshTokenTime
		});

		if (!keys.ContainsKey(TwitterDistributorConstants.RefreshToken)
			|| string.IsNullOrWhiteSpace(keys[TwitterDistributorConstants.RefreshToken]))
		{
			_logger.LogError("Unable to initialize twitter, missing refresh token");
			return;
		}

		_refreshToken = keys[TwitterDistributorConstants.RefreshToken];
		_nextRefreshTime = DateTime.UtcNow.AddDays(-1);
		if (keys.ContainsKey(TwitterDistributorConstants.RefreshTokenTime)
			&& string.IsNullOrWhiteSpace(keys[TwitterDistributorConstants.RefreshTokenTime]))
		{
			var result = DateTime.TryParse(keys[TwitterDistributorConstants.RefreshTokenTime], out var time);
			if (result)
			{
				_nextRefreshTime = time;
			}
		}
	}

	public void CacheValues()
	{
		_redisCacheService.Set(TwitterDistributorConstants.RefreshToken, _refreshToken, Durations.OneWeekInSeconds);
		_redisCacheService.Set(TwitterDistributorConstants.RefreshTokenTime, _nextRefreshTime.ToString(), Durations.OneWeekInSeconds);
	}

	public async Task RequestTokensFromTwitter()
	{
		var formData = new List<KeyValuePair<string, string>>
		{
			new("refresh_token", _refreshToken!),
			new("grant_type", "refresh_token"),
			new("scope", "offline.access tweet.read tweet.write users.read")
		};

		string basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

		_accessTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthHeader);

		var response = await _accessTokenClient.PostAsync("", new FormUrlEncodedContent(formData));

		if (response.IsSuccessStatusCode)
		{
			var responseData = JsonSerializer.Deserialize<TwitterRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			_accessToken = responseData!.AccessToken;
			_refreshToken = responseData.RefreshToken;
			_nextRefreshTime = DateTime.UtcNow.AddSeconds(RefreshTokenDuration);
			CacheValues();
		}
	}

	private static string GenerateTwitterMessage(IPostable post)
	{
		var hasLink = !string.IsNullOrWhiteSpace(post.Link);
		
		var body = post.Group switch
		{
			PostGroups.Submission => post.Title,
			PostGroups.Publication => post.Title,
			_ => post.Body
		};

		body = body.Cap(280 - (hasLink ? 25 : 0)) ?? ""; // Twitter always makes links 23, make it 25 just in case
		
		if (hasLink)
		{
			body = $"{body}\n{post.Link}";
		}

		return body;
	}
}

public class TwitterDistributorConstants
{
	public static string RefreshToken = "TwitterRefreshToken";
	public static string RefreshTokenTime = "TwitterRefreshTokenTime";
}

public class TwitterRefreshTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = "";
}
