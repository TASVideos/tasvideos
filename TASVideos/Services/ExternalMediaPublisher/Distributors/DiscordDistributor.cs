using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class DiscordDistributor : IPostDistributor
	{
		private const int BufferSize = 65535;

		private readonly ILogger _logger;
		private readonly AppSettings.DiscordConnection _settings;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ClientWebSocket _gateway;

		private readonly CancellationTokenSource _cancellationTokenSource = new();

		private int _sequenceNumber = -1;
		private bool _heartbeatAcknowledged;

		// ReSharper disable once NotAccessedField.Local
		private Timer? _heartbeatTimer;

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public DiscordDistributor(IOptions<AppSettings> appSettings, ILogger<DiscordDistributor> logger, IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_settings = appSettings.Value.Discord;
			_httpClientFactory = httpClientFactory;

			_gateway = new ClientWebSocket();

			if (string.IsNullOrWhiteSpace(appSettings.Value.Discord.AccessToken))
			{
				logger.Log(LogLevel.Warning, "Discord bot access key not provided. Bot initialization skipped");
				return;
			}

			ConnectWebsocket();

			Console.CancelKeyPress += CloseWebsocket!;
		}

		private async void ConnectWebsocket()
		{
			var receiveBuffer = WebSocket.CreateClientBuffer(BufferSize, 4096);

			Uri gatewayUri = new("wss://gateway.discord.gg/?v=6&encoding=json");
			CancellationToken cancellationToken = _cancellationTokenSource.Token;

			await _gateway.ConnectAsync(gatewayUri, cancellationToken);

			while (_gateway.State == WebSocketState.Open || _gateway.State == WebSocketState.Connecting)
			{
				WebSocketReceiveResult result = await _gateway.ReceiveAsync(receiveBuffer, cancellationToken);

				string message = Encoding.ASCII.GetString(receiveBuffer.Array!, receiveBuffer.Offset, result.Count);

				while (!result.EndOfMessage)
				{
					result = await _gateway.ReceiveAsync(receiveBuffer, cancellationToken);
					message += Encoding.ASCII.GetString(receiveBuffer.Array!, receiveBuffer.Offset, result.Count);
				}

				HandleMessage(message);
			}
		}

		private async void CloseWebsocket(object sender, ConsoleCancelEventArgs args)
		{
			await ShutDown(WebSocketCloseStatus.NormalClosure, "Server shutting down.");
		}

		private void HandleMessage(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				_logger.LogDebug($"-> Discord: {message}");

				JObject messageObject = JObject.Parse(message);

				if (messageObject.ContainsKey("op"))
				{
					switch (messageObject["op"]!.Value<int>())
					{
						case 1:     // Heartbeat
							ParseHeartbeat(messageObject);
							break;
						case 10:    // Hello
							ParseHello(messageObject);
							Identify();
							break;
						case 11:    // Heartbeat Acknowledge
							_heartbeatAcknowledged = true;
							break;
					}
				}
			}
		}

		private async void Identify()
		{
			JObject properties = new()
			{
				{"$os", "linux"}, {"$browser", "none"}, {"$device", "none"}
			};

			JObject d = new()
			{
				{"token", _settings.AccessToken}, {"properties", properties}
			};

			JObject identifyObject = new()
			{
				{"op", 2}, {"d", d}, {"intents", 0}
			};

			await _gateway.SendAsync(Encoding.ASCII.GetBytes(identifyObject.ToString()), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
		}

		private void ParseHello(JObject helloObject)
		{
			int heartbeatTime = helloObject["d"]!["heartbeat_interval"]!.Value<int>();

			_heartbeatTimer = new Timer(_ => SendHeartbeat(), null, heartbeatTime, heartbeatTime);
			_heartbeatAcknowledged = true;
		}

		private void ParseHeartbeat(JObject heartbeatObject)
		{
			_sequenceNumber = heartbeatObject["d"]!.Value<int>();
		}

		private async void SendHeartbeat()
		{
			if (!_heartbeatAcknowledged)
			{
				await ShutDown(WebSocketCloseStatus.ProtocolError, "Did not receive heartbeat acknowledgement from server.");

				ConnectWebsocket();

				return;
			}

			_heartbeatAcknowledged = false;

			JObject heartbeatObject = new() { { "op", 1 } };

			if (_sequenceNumber == -1)
			{
				heartbeatObject.Add("d", null);
			}
			else
			{
				heartbeatObject.Add("d", _sequenceNumber);
			}

			await _gateway!.SendAsync(Encoding.ASCII.GetBytes(heartbeatObject.ToString()), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
		}

		public async Task ShutDown(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string closureMessage = "Shutting down.")
		{
			if (_gateway.State != WebSocketState.Closed && _gateway.State != WebSocketState.CloseReceived && _gateway.State != WebSocketState.CloseSent)
			{
				Console.WriteLine("Shutdown signal received, closing gateway.");
				await _gateway!.CloseAsync(closeStatus, closureMessage, _cancellationTokenSource.Token);

				System.Diagnostics.Stopwatch stopwatch = new();

				stopwatch.Start();

				// Give it a few seconds.
				while (_gateway.State != WebSocketState.Closed && stopwatch.ElapsedMilliseconds < 10000)
				{ }

				stopwatch.Stop();
			}
		}

		public async void Post(IPostable post)
		{
			DiscordMessage discordMessage = new(post);

			HttpContent messageContent = new StringContent(discordMessage.Serialize(), Encoding.UTF8, "application/json");

			using HttpClient httpClient = _httpClientFactory.CreateClient("Discord");
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _settings.AccessToken);

			var response = await httpClient!.PostAsync($"channels/{(post.Type == PostType.Administrative ? _settings.PrivateChannelId : _settings.PublicChannelId)}/messages", messageContent, _cancellationTokenSource.Token);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Discord.");
				_logger.LogError(await response.Content.ReadAsStringAsync());
			}
		}
	}

	internal class DiscordMessage
	{
		private IPostable Post { get; }

		public DiscordMessage(IPostable post)
		{
			Post = post;
		}

		public string Serialize()
		{
			var serializedMessage = new JObject
			{
				{ "content", GenerateContentMessage(Post) }
			};

			var embedObject = new JObject
			{
				{ "title", Post.Title }, { "description", Post.Body }, { "url", Post.Link }
			};

			serializedMessage.Add("embed", embedObject);

			return serializedMessage.ToString();
		}

		private static string GenerateContentMessage(IPostable post)
		{
			var contentMessageBuilder = new StringBuilder();

			switch (post.Group)
			{
				case PostGroups.Forum:
					contentMessageBuilder.Append("Forum Update");
					break;
				case PostGroups.Submission:
					contentMessageBuilder.Append("Submission Update");
					break;
				case PostGroups.UserFiles:
					contentMessageBuilder.Append("Userfile Update");
					break;
				case PostGroups.UserManagement:
					contentMessageBuilder.Append("User Update");
					break;
				case PostGroups.Wiki:
					contentMessageBuilder.Append("Wiki Update");
					break;
				default:
					contentMessageBuilder.Append("Update");
					break;
			}

			if (!string.IsNullOrWhiteSpace(post.User))
			{
				contentMessageBuilder.Append($" from {post.User}");
			}

			return contentMessageBuilder.ToString();
		}
	}
}