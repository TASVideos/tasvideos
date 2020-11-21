using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class TwitterDistributor : IPostDistributor
	{
		private readonly ILogger _logger;
		private readonly AppSettings.TwitterConnection _settings;
		private readonly IHttpClientFactory _httpClientFactory;

		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private readonly bool _configured = false;

		private Random _rng = new Random();

		public IEnumerable<PostType> Types => new[] { PostType.Announcement };

		public TwitterDistributor(IOptions<AppSettings> appSettings, ILogger<TwitterDistributor> logger, IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_settings = appSettings.Value.Twitter;
			_httpClientFactory = httpClientFactory;

			if (string.IsNullOrWhiteSpace(appSettings.Value.Twitter.ApiBase) ||
				string.IsNullOrWhiteSpace(appSettings.Value.Twitter.ConsumerKey) ||
				string.IsNullOrWhiteSpace(appSettings.Value.Twitter.ConsumerSecret) ||
				string.IsNullOrWhiteSpace(appSettings.Value.Twitter.AccessToken) ||
				string.IsNullOrWhiteSpace(appSettings.Value.Twitter.TokenSecret))
			{
				logger.Log(LogLevel.Warning, "Twitter access tokens were not provided.  The Twitter post distributor will not function.");
				return;
			}

			_configured = true;
		}

		public async void Post(IPostable post)
		{
			// If the Twitter credentials were not configured correctly, just leave the function.
			if (_configured)
			{
				// Generate the Twitter message.  This can easily be configured later to better represent the way we want posts to look.
				string twitterMessage = $"{post.Body}{Environment.NewLine}{post.Link}";

				string nonce = GenerateNonce();
				string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

				string signatureBaseString = CalculateSignatureBaseString("POST", $"{_settings.ApiBase}statuses/update.json", nonce, timestamp, twitterMessage);
				string signature = CalculateSignature(signatureBaseString);

				string authorizationHeaderValue = CalculateOAuthAuthorizationString(nonce, timestamp, signature);

				using HttpClient httpClient = _httpClientFactory.CreateClient("Twitter");
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorizationHeaderValue);

				List<KeyValuePair<string, string>> formFields = new List<KeyValuePair<string, string>>();
				formFields.Add(new KeyValuePair<string, string>("status", twitterMessage));

				var response = await httpClient.PostAsync($"statuses/update.json", new FormUrlEncodedContent(formFields));

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Twitter.");

					_logger.LogError($"Signature Base String: {signatureBaseString}");
					_logger.LogError($"Signature: {signature}");
					_logger.LogError(httpClient.DefaultRequestHeaders.Authorization.ToString());

					_logger.LogError(await response.Content.ReadAsStringAsync());
				}
			}
		}

		private string CalculateSignatureBaseString(string method, string url, string nonceString, string timestamp, string statusMessage)
		{
			string baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}";

			string entities = UrlEncode("include_entities", "true");
			string consumerKey = UrlEncode("oauth_consumer_key", _settings.ConsumerKey);
			string nonce = UrlEncode("oauth_nonce", nonceString);
			string signatureMethod = UrlEncode("oauth_signature_method", "HMAC-SHA1");
			string oauthTimestamp = UrlEncode("oauth_timestamp", timestamp);
			string oauthToken = UrlEncode("oauth_token", _settings.AccessToken);
			string oauthVersion = UrlEncode("oauth_version", "1.0");
			string status = UrlEncode("status", statusMessage);

			string parameterString = $"{entities}&{consumerKey}&{nonce}&{signatureMethod}&{oauthTimestamp}&{oauthToken}&{oauthVersion}&{status}";

			baseString = $"{baseString}&{Uri.EscapeDataString(parameterString)}";

			return baseString;
		}

		private string CalculateSignature(string baseString)
		{
			byte[] hashedBytes;
			string signingKey = $"{Uri.EscapeDataString(_settings.ConsumerSecret)}&{Uri.EscapeDataString(_settings.TokenSecret)}";

			using HMACSHA1 hmac = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(signingKey));
			hashedBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(baseString));

			return Convert.ToBase64String(hashedBytes);
		}

		private string CalculateOAuthAuthorizationString (string nonce, string timestamp, string signature)
		{
			StringBuilder authorizationString = new StringBuilder();

			authorizationString.Append(KVPair("oauth_consumer_key", _settings.ConsumerKey, false));
			authorizationString.Append(KVPair("oauth_token", _settings.AccessToken, false));
			authorizationString.Append(KVPair("oauth_signature_method", "HMAC-SHA1", false));
			authorizationString.Append(KVPair("oauth_timestamp", timestamp, false));
			authorizationString.Append(KVPair("oauth_nonce", nonce, false));
			authorizationString.Append(KVPair("oauth_version", "1.0", false));
			authorizationString.Append(KVPair("oauth_signature", signature, true));

			return authorizationString.ToString();
		}

		private string UrlEncode (string left, string right)
		{
			return $"{Uri.EscapeDataString(left)}={Uri.EscapeDataString(right)}";
		}

		private string KVPair (string left, string right, bool final)
		{
			return $"{left}=\"{right}\"{(final ? "" : ",")}";
		}

		private string GenerateNonce()
		{
			string NonceCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

			StringBuilder outputString = new StringBuilder();

			for (int i = 0; i < 32; ++i)
			{
				outputString.Append(NonceCharacters.Substring(_rng.Next(0, NonceCharacters.Length), 1));
			}

			return outputString.ToString();
		}
	}
}
