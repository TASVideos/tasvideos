using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class XDistributorV2 : IPostDistributor
{
	private static System.Timers.Timer? _timer = null;
	private static readonly object TimeLock = new();

	private readonly HttpClient _xClient = null!;
	private readonly HttpClient _accessTokenClient = null!;
	private readonly AppSettings.XConnectionV2 _settings;
	private readonly ILogger<XDistributorV2> _logger = null!;

	private XTokenDetails _xTokenDetails = new();

	private readonly string _tokenStorageFileName = null!;

	private readonly TimeSpan _accessTokenDuration = new (1, 59, 30);  // Two hours minus thirty seconds.
	private readonly TimeSpan _refreshTokenDuration = new (177, 12, 0, 0); // Refresh tokens last "six months", so this is just a bit less than that.

	public XDistributorV2(
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<XDistributorV2> logger)
	{
		_settings = appSettings.XV2;
		if (!_settings.IsEnabled())
		{
			return;
		}
		
		_xClient = httpClientFactory.CreateClient(HttpClients.XV2);
		_accessTokenClient = httpClientFactory.CreateClient(HttpClients.XAuth);
		_logger = logger;

		_tokenStorageFileName = Path.Combine(Path.GetTempPath(), "twitter.json");
		RetrieveTokenInformation();

		lock (TimeLock)
		{
			if (_timer is null)
			{
				_timer = new System.Timers.Timer();
				_timer.Elapsed += (_, _) =>
				{
					try
					{
						_logger.LogInformation("Automatically refreshing x tokens on a timer");
						RequestTokensFromX().Wait();
					}
					catch (Exception ex)
					{
						_logger.LogError("An error occured getting x tokens from timer ex: {ex}", ex);
					}
				};
				_timer.Interval = Durations.OneHourInMilliseconds;
				_timer.AutoReset = true;
				_timer.Start();
			}
		}
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public bool IsEnabled()
	{
		if (!_settings.IsEnabled())
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(_xTokenDetails.AccessToken))
		{
			// Try to get X token information from the local file.
			if (File.Exists(_tokenStorageFileName))
			{
				RetrieveTokenInformation();
			}
		}

		return !string.IsNullOrWhiteSpace(_xTokenDetails.AccessToken);
	}

	public async Task Post(IPostable post)
	{
		await RefreshTokensIfExpired();

		if (!IsEnabled())
		{
			return;
		}

		_xClient.SetBearerToken(_xTokenDetails.AccessToken);

		var tweetData = new
		{
			text = GenerateXMessage(post)
		};

		var response = await _xClient.PostAsync("", tweetData.ToStringContent());

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("Error sending tweet: {reasonPhrase}", response.ReasonPhrase);
		}
	}

	private void RetrieveTokenInformation()
	{
		if (!File.Exists(_tokenStorageFileName))
		{
			_logger.LogWarning("{_tokenStorageFileName} not found, x is likely not to work", _tokenStorageFileName);
			return;
		}

		string tokenText = File.ReadAllText(_tokenStorageFileName);

		if (!string.IsNullOrWhiteSpace(tokenText))
		{
			try
			{
				_xTokenDetails = JsonSerializer.Deserialize<XTokenDetails>(tokenText) ?? new XTokenDetails();

				if (DateTime.UtcNow > _xTokenDetails.RefreshTokenExpiry)
				{
					_xTokenDetails.RefreshToken = "";
				}
			}
			catch (Exception) { }
		}
	}

	private async Task RefreshTokensIfExpired()
	{
		if (string.IsNullOrWhiteSpace(_xTokenDetails.AccessToken) ||
			DateTime.UtcNow > _xTokenDetails.AccessTokenExpiry)
		{
			await RequestTokensFromX();
		}
	}

	private async Task RequestTokensFromX()
	{
		var refreshResult = await RequestTokensFromX(_xTokenDetails.RefreshToken);
		if (!refreshResult)
		{
			await RequestTokensFromX(_settings.OneTimeRefreshToken);
		}
	}

	private async Task<bool> RequestTokensFromX (string refreshToken)
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
			var responseData = JsonSerializer.Deserialize<XRefreshTokenResponse>(await response.Content.ReadAsStringAsync());

			if (responseData is null)
			{
				_logger.LogError("Got a successful response from X, but received no tokens!");
			}
			else
			{
				_xTokenDetails.AccessToken = responseData.AccessToken;
				_xTokenDetails.AccessTokenExpiry = DateTime.UtcNow + _accessTokenDuration;

				_xTokenDetails.RefreshToken = responseData.RefreshToken;
				_xTokenDetails.RefreshTokenExpiry = DateTime.UtcNow + _refreshTokenDuration;

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

			// Unrecoverable error, we need to generate new tokens anyways so we disable X for now.
			_xTokenDetails.AccessToken = "";
			_xTokenDetails.RefreshToken = "";
		}

		return retVal;
	}

	private static string GenerateXMessage(IPostable post)
	{
		var hasLink = !string.IsNullOrWhiteSpace(post.Link);
		
		var body = post.Group switch
		{
			PostGroups.Submission => post.Title,
			PostGroups.Publication => post.Title,
			_ => post.Body
		};

		body = body.Cap(280 - (hasLink ? 25 : 0)) ?? ""; // X always makes links 23, make it 25 just in case
		
		if (hasLink)
		{
			body = $"{body}\n{post.Link}";
		}

		return body;
	}

	// Write the XTokenDetails object to the file.
	private async Task StoreValues()
	{
		try
		{
			await File.WriteAllTextAsync(_tokenStorageFileName, JsonSerializer.Serialize(_xTokenDetails));
		}
		catch (Exception ex)
		{
			_logger.LogError("Critical error writing X access token details to the temporary file. Additional information: {message}", ex.Message);
		}
	}
}

public class XRefreshTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = "";

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = "";
}

public class XTokenDetails
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
