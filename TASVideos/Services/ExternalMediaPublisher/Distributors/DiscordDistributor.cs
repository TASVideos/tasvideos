using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TASVideos.Api.Requests;
using TASVideos.Pages.Profile;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class DiscordDistributor : IPostDistributor
	{
		ILogger _logger;

		private readonly AppSettings.DiscordConnection _settings;

		static readonly HttpClient _httpClient = new HttpClient();

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public DiscordDistributor (IOptions<AppSettings> appSettings, ILogger<DiscordDistributor> logger)
		{
			_logger = logger;
			_settings = appSettings.Value.Discord;
		}

		public async void Post (IPostable post)
		{
			if (_httpClient == null)
			{
				return;
			}

			_logger.LogInformation($"Access token: {_settings.AccessToken}, API Base: {_settings.ApiBase}, Channel ID: {_settings.ChannelId}");

			DiscordMessage discordMessage = new DiscordMessage(post.Title, post.Body, post.Link);

			HttpRequestMessage apiRequest = new HttpRequestMessage();
			apiRequest.Method = HttpMethod.Post;
			apiRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AccessToken);
			apiRequest.Content = new StringContent(discordMessage.Serialize(), Encoding.UTF8, "application/json");
			apiRequest.RequestUri = new Uri($"{_settings.ApiBase}/channels/{_settings.ChannelId}/messages");

			var response = await _httpClient!.SendAsync(apiRequest).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Discord.");
				_logger.LogError(await response.Content.ReadAsStringAsync());
			}
		}

		//public async Task<bool> Authorize ()
		//{
		//	HttpRequestMessage authRequest = new HttpRequestMessage();
		//	authRequest.Method = HttpMethod.Post;
		//	authRequest.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
		//	authRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}")));
		//	authRequest.Content = new FormUrlEncodedContent(
		//		new KeyValuePair<string, string>[]
		//		{
		//			new KeyValuePair<string, string>("grant_type", "client_credentials"),
		//			new KeyValuePair<string, string>("scope", "bot")
		//		});

		//	var authResponse = await _httpClient.SendAsync(authRequest).ConfigureAwait(false);

		//	if (authResponse.IsSuccessStatusCode)
		//	{
		//		JObject authResponseObject = JObject.Parse(await authResponse.Content.ReadAsStringAsync());
		//		if (authResponseObject.ContainsKey("access_token") && authResponseObject.ContainsKey("expires_in"))
		//		{
		//			OAuthToken = authResponseObject["access_token"]!.Value<string>();
		//			TokenExpiry = DateTime.Now + new TimeSpan(0, 0, authResponseObject["expires_in"]!.Value<int>());
		//		}
		//		else
		//		{
		//			// Error with authentication.
		//			_logger.LogError($"{DateTime.Now}: Could not retrieve access token from Discord.");
		//			_logger.LogError(await authResponse.Content.ReadAsStringAsync());
		//		}
		//	}
		//	else
		//	{
		//		// Error with authentication.
		//		_logger.LogError($"{DateTime.Now}: Could not retrieve access token from Discord.");
		//		_logger.LogError(await authResponse.Content.ReadAsStringAsync());
		//	}

		//	return authResponse.IsSuccessStatusCode;
		//}
	}

	internal class DiscordMessage
	{
		string PostTitle { get; set; }
		string PostBody { get; set; }
		string PostUrl { get; set; }
		string PostUser { get; set; } = "";


		public DiscordMessage (string postTitle, string postBody, string postUrl)
		{
			this.PostTitle = postTitle;
			this.PostBody = postBody;
			this.PostUrl = postUrl;
		}

		public string Serialize ()
		{
			JObject serializedMessage = new JObject();
			JObject embedObject = new JObject();

			serializedMessage.Add("content", $"New post from {PostUser}");

			embedObject.Add("title", PostTitle);
			embedObject.Add("description", PostBody);
			embedObject.Add("url", PostUrl);

			serializedMessage.Add("embed", embedObject);

			return serializedMessage.ToString();
		}
	}
}
