﻿using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Settings;
using TASVideos.Core.HttpClientExtensions;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;

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

		string channel;

		if (post.Type == PostType.Administrative)
		{
			channel = post.Group == PostGroups.UserManagement
				? _settings.PrivateUserChannelId
				: _settings.PrivateChannelId;
		}
		else
		{
			if (post.Group == PostGroups.Game)
			{
				channel = _settings.PublicGameChannelId;
			}
			else if (post.Group is PostGroups.Publication
					or PostGroups.Submission
					or PostGroups.UserFiles)
			{
				channel = _settings.PublicTasChannelId;
			}
			else
			{
				channel = _settings.PublicChannelId;
			}
		}

		var response = await _client.PostAsync($"channels/{channel}/messages", messageContent);
		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError(
				"[{timestamp}] An error occurred sending a message to Discord. Response: {response}",
				DateTime.UtcNow,
				await response.Content.ReadAsStringAsync());
		}
	}

	private class DiscordMessage
	{
		// Generate the Discord message letting Discord take care of the Embed from Open Graph Metadata
		public DiscordMessage(IPostable post)
		{
			var body = string.IsNullOrWhiteSpace(post.Body) ? "" : $" ({post.Body})";
			if (string.IsNullOrWhiteSpace(post.Link))
			{
				Content = $"{post.Title}{body}";
			}
			else
			{
				var link = post.Type == PostType.Announcement ? post.Link : $"<{post.Link}>";
				Content = string.IsNullOrWhiteSpace(post.FormattedTitle)
					? $"{post.Title}{body} {link}"
					: $"{string.Format(post.FormattedTitle, link)}{body}";
			}
		}

		[JsonPropertyName("content")]
		public string Content { get; set; }
	}
}
