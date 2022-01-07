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
			StringContent messageContent;

			if (String.IsNullOrWhiteSpace(post.Announcement))
			{
				messageContent = new CustomDiscordMessage(post).ToStringContent();
			}
			else
			{
				messageContent = new DiscordMessage(post).ToStringContent();
			}

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

		private class CustomDiscordMessage
		{
			public CustomDiscordMessage(IPostable post)
			{
				Content = GenerateContentMessage(post.Group, post.User);
				Embed = new()
				{
					Title = post.Title,
					Url = post.Link,
					Description = post.Body
				};
			}

			[JsonProperty("content")]
			public string Content { get; }

			[JsonProperty("embed")]
			public EmbedData Embed { get; }

			public class EmbedData
			{
				[JsonProperty("title")]
				public string Title { get; init; } = "";

				[JsonProperty("description")]
				public string Description { get; init; } = "";

				[JsonProperty("url")]
				public string Url { get; init; } = "";
			}

			private static string GenerateContentMessage(string? group, string? user)
			{
				string message = group switch
				{
					PostGroups.Forum => "Forum Update",
					PostGroups.Submission => "Submission Update",
					PostGroups.UserFiles => "Userfile Update",
					PostGroups.UserManagement => "User Update",
					PostGroups.Wiki => "Wiki Update",
					_ => "Update"
				};

				return !string.IsNullOrWhiteSpace(user)
					? message + $" from {user}"
					: message;
			}
		}

		private class DiscordMessage
		{
			// Generate the Discord message letting Discord take care of the Embed from Open Graph Metadata
			public DiscordMessage(IPostable post)
			{
				if (post.Announcement == "New Forum Topic" || post.Announcement == "New Forum Post")
				{
					Content = post.Link;
				}
				else
				{
					Content = $"{post.Announcement}\n{post.Link}";
				}
			}

			[JsonProperty("content")]
			public string Content { get; }
		}
	}
}
