using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class TwitterDistributor : IPostDistributor
{
	private readonly HttpClient _client;
	private readonly AppSettings.TwitterConnection _settings;
	private readonly ILogger<TwitterDistributor> _logger;

	private static readonly Random Rng = new();

	public TwitterDistributor(
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<TwitterDistributor> logger)
	{
		_settings = appSettings.Twitter;
		_client = httpClientFactory.CreateClient(HttpClients.Twitter);
		_logger = logger;
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public async Task Post(IPostable post)
	{
		if (!_settings.IsEnabled())
		{
			return;
		}

		// Generate the Twitter message letting Twitter take care of the Embed from Open Graph Metadata
		string twitterMessage = GenerateTwitterMessage(post);

		string nonce = GenerateNonce();
		string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

		string signatureBaseString = CalculateSignatureBaseString("POST", $"{_settings.ApiBase}statuses/update.json", nonce, timestamp, twitterMessage);
		string signature = CalculateSignature(signatureBaseString);

		string oathToken = CalculateOAuthAuthorizationString(nonce, timestamp, signature);
		_client.SetOAuthToken(oathToken);

		var formFields = new List<KeyValuePair<string?, string?>>
			{
				new ("status", twitterMessage)
			};

		var response = await _client.PostAsync("statuses/update.json", new FormUrlEncodedContent(formFields));

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Twitter.");
			_logger.LogError($"Signature Base String: {signatureBaseString}");
			_logger.LogError($"Signature: {signature}");
			_logger.LogError(_client.DefaultRequestHeaders.Authorization?.ToString());
			_logger.LogError(await response.Content.ReadAsStringAsync());
		}
	}

	private static string GenerateTwitterMessage(IPostable post)
	{
		return post.Group switch
		{
			PostGroups.Submission => $"{post.Announcement}\n{post.Link}",
			_ => ""
		};
	}

	private string CalculateSignatureBaseString(string method, string url, string nonceString, string timestamp, string statusMessage)
	{
		string baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}";

		string consumerKey = UrlEncode("oauth_consumer_key", _settings.ConsumerKey);
		string nonce = UrlEncode("oauth_nonce", nonceString);
		string signatureMethod = UrlEncode("oauth_signature_method", "HMAC-SHA1");
		string oauthTimestamp = UrlEncode("oauth_timestamp", timestamp);
		string oauthToken = UrlEncode("oauth_token", _settings.AccessToken);
		string oauthVersion = UrlEncode("oauth_version", "1.0");
		string status = UrlEncode("status", statusMessage);

		string parameterString = $"{consumerKey}&{nonce}&{signatureMethod}&{oauthTimestamp}&{oauthToken}&{oauthVersion}&{status}";

		baseString = $"{baseString}&{Uri.EscapeDataString(parameterString)}";

		return baseString;
	}

	private string CalculateSignature(string baseString)
	{
		string signingKey = $"{Uri.EscapeDataString(_settings.ConsumerSecret)}&{Uri.EscapeDataString(_settings.TokenSecret)}";

		using HMACSHA1 hmac = new(Encoding.UTF8.GetBytes(signingKey));
		byte[] hashedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));

		return Convert.ToBase64String(hashedBytes);
	}

	private string CalculateOAuthAuthorizationString(string nonce, string timestamp, string signature)
	{
		var authorizationString = new StringBuilder();

		authorizationString.Append(KvPair("oauth_consumer_key", _settings.ConsumerKey, false));
		authorizationString.Append(KvPair("oauth_token", _settings.AccessToken, false));
		authorizationString.Append(KvPair("oauth_signature_method", "HMAC-SHA1", false));
		authorizationString.Append(KvPair("oauth_timestamp", timestamp, false));
		authorizationString.Append(KvPair("oauth_nonce", nonce, false));
		authorizationString.Append(KvPair("oauth_version", "1.0", false));
		authorizationString.Append(KvPair("oauth_signature", signature, true));

		return authorizationString.ToString();
	}

	private static string UrlEncode(string left, string right)
	{
		return $"{Uri.EscapeDataString(left)}={Uri.EscapeDataString(right)}";
	}

	private static string KvPair(string left, string right, bool final)
	{
		return $"{Uri.EscapeDataString(left)}=\"{Uri.EscapeDataString(right)}\"{(final ? "" : ", ")}";
	}

	private string GenerateNonce()
	{
		string nonceCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		var outputString = new StringBuilder();
		for (int i = 0; i < 32; ++i)
		{
			outputString.Append(nonceCharacters.AsSpan(Rng.Next(0, nonceCharacters.Length), 1));
		}

		return outputString.ToString();
	}
}
