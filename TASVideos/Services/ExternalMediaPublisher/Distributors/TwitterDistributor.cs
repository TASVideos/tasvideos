using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Core.Settings;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class TwitterDistributor : IPostDistributor
	{
		private readonly ILogger _logger;
		private readonly AppSettings.TwitterConnection _settings;
		private readonly IHttpClientFactory _httpClientFactory;

		private readonly CancellationTokenSource _cancellationTokenSource = new ();

		private readonly bool _configured;

		private readonly Random _rng = new ();

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
				string twitterMessage = GenerateTwitterMessage(post);

				string nonce = GenerateNonce();
				string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

				string signatureBaseString = CalculateSignatureBaseString("POST", $"{_settings.ApiBase}statuses/update.json", nonce, timestamp, twitterMessage);
				string signature = CalculateSignature(signatureBaseString);

				string authorizationHeaderValue = CalculateOAuthAuthorizationString(nonce, timestamp, signature);

				using HttpClient httpClient = _httpClientFactory.CreateClient("Twitter");
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorizationHeaderValue);

				var formFields = new List<KeyValuePair<string?, string?>>
				{
					new ("status", twitterMessage)
				};

				var response = await httpClient.PostAsync("statuses/update.json", new FormUrlEncodedContent(formFields));

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

		private static string GenerateTwitterMessage(IPostable post)
		{
			string twitterMessage = "";

			switch (post.Group)
			{
				case PostGroups.Submission:
					twitterMessage = $"{post.Title} - {post.Link}";
					break;
			}

			return twitterMessage;
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

			using HMACSHA1 hmac = new (Encoding.UTF8.GetBytes(signingKey));
			byte[] hashedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));

			return Convert.ToBase64String(hashedBytes);
		}

		private string CalculateOAuthAuthorizationString(string nonce, string timestamp, string signature)
		{
			StringBuilder authorizationString = new ();

			authorizationString.Append(KVPair("oauth_consumer_key", _settings.ConsumerKey, false));
			authorizationString.Append(KVPair("oauth_token", _settings.AccessToken, false));
			authorizationString.Append(KVPair("oauth_signature_method", "HMAC-SHA1", false));
			authorizationString.Append(KVPair("oauth_timestamp", timestamp, false));
			authorizationString.Append(KVPair("oauth_nonce", nonce, false));
			authorizationString.Append(KVPair("oauth_version", "1.0", false));
			authorizationString.Append(KVPair("oauth_signature", signature, true));

			return authorizationString.ToString();
		}

		private static string UrlEncode(string left, string right)
		{
			return $"{Uri.EscapeDataString(left)}={Uri.EscapeDataString(right)}";
		}

		private static string KVPair(string left, string right, bool final)
		{
			return $"{Uri.EscapeDataString(left)}=\"{Uri.EscapeDataString(right)}\"{(final ? "" : ", ")}";
		}

		private string GenerateNonce()
		{
			string nonceCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

			StringBuilder outputString = new ();

			for (int i = 0; i < 32; ++i)
			{
				outputString.Append(nonceCharacters.Substring(_rng.Next(0, nonceCharacters.Length), 1));
			}

			return outputString.ToString();
		}
	}
}
