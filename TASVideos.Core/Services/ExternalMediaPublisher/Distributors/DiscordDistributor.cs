using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Settings;
using Newtonsoft.Json;
using TASVideos.Core.HttpClientExtensions;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors
{
	public sealed class DiscordDistributor : IPostDistributor
	{
		private readonly HttpClient _client;
		private readonly AppSettings.DiscordConnection _settings;
		private readonly ILogger<DiscordDistributor> _logger;

		public DiscordDistributor(
			AppSettings appSettings,
			IHttpClientFactory httpClientFactory,
			ILogger<DiscordDistributor> logger)
		{
			_settings = appSettings.Discord;
			_client = httpClientFactory.CreateClient(HttpClients.Discord)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.Discord} client");
			_client.SetBotToken(_settings.AccessToken);
			_logger = logger;
		}

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public async Task Post(IPostable post)
		{
			if (!_settings.IsEnabled())
			{
				return;
			}

			var messageContent = new DiscordMessage(post).ToStringContent();
			
			string channel = post.Type == PostType.Administrative
				? _settings.PrivateChannelId
				: _settings.PublicChannelId;

			var response = await _client.PostAsync($"channels/{channel}/messages", messageContent);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Discord.");
				_logger.LogError(await response.Content.ReadAsStringAsync());
			}
		}

		private class DiscordMessage
		{
			// Generate the Discord message letting Discord take care of the Embed from Open Graph Metadata
			public DiscordMessage(IPostable post)
			{
				Content = $"{post.Announcement}\n{post.Link}";
			}

			[JsonProperty("content")]
			public string Content { get; }
		}
	}
}
