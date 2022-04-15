using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
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

	private TwitterTokenDetails _twitterTokenDetails = new TwitterTokenDetails();

	private string? _accessToken;
	private string? _refreshToken;
	private DateTime? _nextAccessTokenRefreshTime;
	private DateTime? _nextRefreshTokenRefreshTime;

	private string _tokenStorageFileName;

	private TimeSpan AccessTokenDuration = new TimeSpan(1, 59, 30);  // Two hours minus thirty seconds in seconds.
	private TimeSpan RefreshTokenDuration = new TimeSpan(179, 12, 0, 0); // Refresh tokens last "six months", so this is just a bit less than that.

	public TwitterDistributorV2(
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<TwitterDistributorV2> logger)
	{
		_settings = appSettings.TwitterV2;
		_twitterClient = httpClientFactory.CreateClient(HttpClients.TwitterV2);
		_accessTokenClient = httpClientFactory.CreateClient(HttpClients.TwitterAuth);
		_logger = logger;

		_tokenStorageFileName = Path.Combine(Path.GetTempPath(), "twitter.json");

		// Try to get Twitter token information from the local file.
		if (File.Exists(_tokenStorageFileName))
		{
			RetrieveTokenInformation();
		}

		// If the local file doesn't exist, or if there was no data to parse, use the OneTimeRefreshToken and hope.
		if (_twitterTokenDetails.RefreshToken == null)
		{
			_twitterTokenDetails.RefreshToken = _settings.OneTimeRefreshToken;
		}
	}


	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public bool IsEnabled() => _settings.IsEnabled() && !string.IsNullOrWhiteSpace(_twitterTokenDetails.AccessToken);

	public void RetrieveTokenInformation()
	{
		string tokenText = File.ReadAllText(_tokenStorageFileName);

		if (tokenText != null)
		{
			try
			{
				_twitterTokenDetails = JsonSerializer.Deserialize<TwitterTokenDetails>(tokenText) ?? new TwitterTokenDetails();
			}
			catch (Exception) { }
		}
	}

	public async Task Post(IPostable post)
	{
		await RefreshTokens();

		if (IsEnabled())
		{
			return;
		}

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
		if (_nextRefreshTokenRefreshTime == null || DateTime.UtcNow > _nextRefreshTokenRefreshTime)
		{
			await RequestTokensFromTwitter(true);
		}
		else if (_nextAccessTokenRefreshTime == null || DateTime.UtcNow > _nextAccessTokenRefreshTime)
		{
			await RequestTokensFromTwitter(false);
		}
	}

	public async Task RequestTokensFromTwitter(bool newRefreshToken)
	{
		// The offline.access scope regenerates the refresh token.  Maybe the refresh token can be used multiple times before it needs refreshing itself?
		var formData = new List<KeyValuePair<string, string>>
		{
			new("refresh_token", _refreshToken!),
			new("grant_type", "refresh_token"),
			new("scope", $"tweet.read tweet.write users.read{(newRefreshToken ? " offline.access" : "")}")
		};

		string basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

		_accessTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuthHeader);

		var response = await _accessTokenClient.PostAsync("", new FormUrlEncodedContent(formData));

		if (response.IsSuccessStatusCode)
		{
			var responseData = JsonSerializer.Deserialize<TwitterRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			_accessToken = responseData!.AccessToken;
			_nextAccessTokenRefreshTime = DateTime.UtcNow + AccessTokenDuration;

			if (newRefreshToken)
			{
				_refreshToken = responseData.RefreshToken;
				_nextRefreshTokenRefreshTime = DateTime.UtcNow + RefreshTokenDuration;
			}

			StoreValues();
		}
		else
		{
			_logger.LogError("Error getting {tokenType} token.  Received HTTP status code {statusCode}.", newRefreshToken ? "refresh" : "access", response.StatusCode);
			_logger.LogError(await response.Content.ReadAsStringAsync());
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

	// Write the TwitterTokenDetails object to the file.
	private void StoreValues()
	{
		try
		{
			File.WriteAllText(_tokenStorageFileName, JsonSerializer.Serialize(_twitterTokenDetails));
		}
		catch (Exception ex)
		{
			_logger.LogError("Critical error writing Twitter access token details to the temporary file. Additional information: {message}", ex.Message);
		}
	}
}

public class TwitterRefreshTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = "";
}

public class TwitterTokenDetails
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";
	[JsonPropertyName("access_token_expiry")]
	public DateTime AccessTokenExpiry { get; set; } = DateTime.MinValue;
	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = "";
	[JsonPropertyName("refresh_token_expiry")]
	public DateTime RefreshTokenExpiry { get; set; } = DateTime.MinValue;
}
