using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class TwitterDistributorV2 : IPostDistributor
{
	private static System.Timers.Timer? _timer = null;
	private static readonly object TimeLock = new();

	private readonly HttpClient _twitterClient;
	private readonly HttpClient _accessTokenClient;
	private readonly AppSettings.TwitterConnectionV2 _settings;
	private readonly ILogger<TwitterDistributorV2> _logger;

	private TwitterTokenDetails _twitterTokenDetails = new();

	private readonly string _tokenStorageFileName;

	private readonly TimeSpan _accessTokenDuration = new (1, 59, 30);  // Two hours minus thirty seconds.
	private readonly TimeSpan _refreshTokenDuration = new (177, 12, 0, 0); // Refresh tokens last "six months", so this is just a bit less than that.

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

		lock (TimeLock)
		{
			if (_timer is null)
			{
				_timer = new System.Timers.Timer();
				_timer.Elapsed += (_, _) =>
				{
					try
					{
						_logger.LogInformation("Automatically refreshing twitter tokens on a timer");
						RequestTokensFromTwitter().Wait();
					}
					catch (Exception ex)
					{
						_logger.LogError("An error occured getting twitter tokens from timer ex: {ex}", ex);
					}
				};
				_timer.Interval = Durations.OneHourInMilliseconds;
				_timer.AutoReset = true;
				_timer.Start();
			}
		}
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public async Task<bool> IsEnabled()
	{
		if (!_settings.IsEnabled())
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(_twitterTokenDetails.AccessToken))
		{
			// Try to get Twitter token information from the local file.
			if (File.Exists(_tokenStorageFileName))
			{
				await RetrieveTokenInformation();
			}
		}

		return !string.IsNullOrWhiteSpace(_twitterTokenDetails.AccessToken);
	}

	public async Task Post(IPostable post)
	{
		await RefreshTokensIfExpired();

		if (!await IsEnabled())
		{
			return;
		}

		_twitterClient.SetBearerToken(_twitterTokenDetails.AccessToken);

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

	private async Task RetrieveTokenInformation()
	{
		string tokenText = await File.ReadAllTextAsync(_tokenStorageFileName);

		if (!string.IsNullOrWhiteSpace(tokenText))
		{
			try
			{
				_twitterTokenDetails = JsonSerializer.Deserialize<TwitterTokenDetails>(tokenText) ?? new TwitterTokenDetails();

				if (DateTime.UtcNow > _twitterTokenDetails.RefreshTokenExpiry)
				{
					_twitterTokenDetails.RefreshToken = "";
				}
			}
			catch (Exception) { }
		}
	}

	private async Task RefreshTokensIfExpired()
	{
		if (string.IsNullOrWhiteSpace(_twitterTokenDetails.AccessToken) ||
			DateTime.UtcNow > _twitterTokenDetails.AccessTokenExpiry)
		{
			await RequestTokensFromTwitter();
		}
	}

	private async Task RequestTokensFromTwitter()
	{
		var refreshResult = await RequestTokensFromTwitter(_twitterTokenDetails.RefreshToken);
		if (!refreshResult)
		{
			await RequestTokensFromTwitter(_settings.OneTimeRefreshToken);
		}
	}

	private async Task<bool> RequestTokensFromTwitter (string refreshToken)
	{
		bool retVal = false;

		if (string.IsNullOrWhiteSpace(refreshToken))
		{
			return false;
		}

		// The offline.access scope regenerates the refresh token.  Maybe the refresh token can be used multiple times before it needs refreshing itself?
		var formData = new List<KeyValuePair<string, string>>
		{
			new("refresh_token", refreshToken),
			new("grant_type", "refresh_token"),
			new("scope", "tweet.read tweet.write users.read offline.access")
		};

		string basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
		_accessTokenClient.SetBasicAuth(basicAuthHeader);

		var response = await _accessTokenClient.PostAsync("", new FormUrlEncodedContent(formData));

		if (response.IsSuccessStatusCode)
		{
			var responseData = JsonSerializer.Deserialize<TwitterRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			if (responseData is null)
			{
				_logger.LogError("Got a successful response from Twitter, but received no tokens!");
			}
			else
			{
				_twitterTokenDetails.AccessToken = responseData.AccessToken;
				_twitterTokenDetails.AccessTokenExpiry = DateTime.UtcNow + _accessTokenDuration;

				_twitterTokenDetails.RefreshToken = responseData.RefreshToken;
				_twitterTokenDetails.RefreshTokenExpiry = DateTime.UtcNow + _refreshTokenDuration;

				await StoreValues();

				retVal = true;
			}
		}
		else
		{
			_logger.LogError("Error getting access tokens.  Received HTTP status code {statusCode}. {newline}{errorMessage}",
				response.StatusCode,
				Environment.NewLine,
				await response.Content.ReadAsStringAsync());

			// Unrecoverable error, we need to generate new tokens anyways so we disable Twitter for now.
			_twitterTokenDetails.AccessToken = "";
			_twitterTokenDetails.RefreshToken = "";
		}

		return retVal;
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
	private async Task StoreValues()
	{
		try
		{
			await File.WriteAllTextAsync(_tokenStorageFileName, JsonSerializer.Serialize(_twitterTokenDetails));
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
