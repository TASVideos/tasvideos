using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

public class TwitterDistributorV2 : IPostDistributor
{
	private readonly HttpClient _twitterClient;
	private readonly AppSettings.TwitterConnection _settings;
	private readonly ILogger<TwitterDistributorV2> _logger;

	public TwitterDistributorV2 (
		AppSettings appSettings,
		IHttpClientFactory httpClientFactory,
		ILogger<TwitterDistributorV2> logger)
	{
		_settings = appSettings.Twitter;
		_twitterClient = httpClientFactory.CreateClient(HttpClients.TwitterV2);
		_logger = logger;

		if (string.IsNullOrEmpty(_settings.AccessTokenV2))
		{
			_logger.LogCritical("No V2 access token found.");
		}

		// This *might* work.  No promises.
		_twitterClient.DefaultRequestHeaders.Authorization = 
			new System.Net.Http.Headers.AuthenticationHeaderValue(
				"Bearer",
				_settings.AccessTokenV2);
	}

	public IEnumerable<PostType> Types => new[] { PostType.Announcement };

	public async Task Post(IPostable post)
	{
		if (!_settings.IsEnabled())
		{
			return;
		}

		HttpRequestMessage tweet = new HttpRequestMessage();
		tweet.Method = HttpMethod.Post;

		List<KeyValuePair<string, string>> tweetData = new List<KeyValuePair<string, string>>()
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
}
