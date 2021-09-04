using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors
{
	public sealed class DiscordDistributor : IPostDistributor, IDisposable
	{
		private static readonly object LockObject = new ();
		private static DiscordBot? DiscordBot;

		public DiscordDistributor(
			AppSettings appSettings,
			ILogger<DiscordDistributor> logger,
			IHttpClientFactory httpClientFactory)
		{
			AppSettings.DiscordConnection settings = appSettings.Discord;

			if (string.IsNullOrWhiteSpace(appSettings.Discord.AccessToken))
			{
				logger.Log(LogLevel.Warning, "Discord bot access key not provided. Bot initialization skipped");
				return;
			}

			lock (LockObject)
			{
				DiscordBot ??= new DiscordBot(httpClientFactory, settings, logger);

				if (!DiscordBot.Connected)
				{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					DiscordBot.ConnectWebsocket();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				}
				Console.CancelKeyPress += DiscordBot.CloseWebsocket!;
				
				// TODO: Include code to close websocket on shutdown.
			}
		}

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public async Task Post(IPostable post)
		{
			if (DiscordBot != null)
			{
				await DiscordBot.Post(post);
			}
		}

		public void Dispose()
		{
			DiscordBot?.ShutDown();
			DiscordBot?.Dispose();
		}
	}

	internal class DiscordMessage
	{
		public DiscordMessage(IPostable post)
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
		public string Content { get; init; }

		[JsonProperty("embed")]
		public EmbedData Embed { get; init; }

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

	internal class DiscordBot : IDisposable
	{
		private const int BufferSize = 65535;

		private readonly CancellationTokenSource _cancellationTokenSource = new();
		private readonly static object HeartbeatLock = new();

		private int _sequenceNumber = -1;
		private bool _heartbeatAcknowledged;

		// ReSharper disable once NotAccessedField.Local
		private readonly AppSettings.DiscordConnection _settings;
		private readonly ClientWebSocket _gateway;
		private readonly IHttpClientFactory _httpClientFactory;
		private Timer? _heartbeatTimer;

		private readonly ILogger _logger;

		// public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public bool Connected => _gateway.State == WebSocketState.Open;

		public DiscordBot(IHttpClientFactory httpClientFactory, AppSettings.DiscordConnection settings, ILogger logger)
		{
			_gateway = new ClientWebSocket();

			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_settings = settings;
		}

		public async Task ConnectWebsocket()
		{
			var receiveBuffer = WebSocket.CreateClientBuffer(BufferSize, 4096);

			Uri gatewayUri = new("wss://gateway.discord.gg/?v=6&encoding=json");
			CancellationToken cancellationToken = _cancellationTokenSource.Token;

			await _gateway.ConnectAsync(gatewayUri, cancellationToken);

			while (_gateway.State is WebSocketState.Open or WebSocketState.Connecting)
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

		public async void CloseWebsocket(object sender, ConsoleCancelEventArgs args)
		{
			await ShutDown(WebSocketCloseStatus.NormalClosure, "Server shutting down.");
		}

		private void HandleMessage(string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				_logger.LogDebug($"-> Discord: {message}");

				JObject messageObject = JObject.Parse(message);

				lock (HeartbeatLock)
				{
					if (messageObject.ContainsKey("op"))
					{
						switch (messageObject["op"]!.Value<int>())
						{
							case 1: // Heartbeat
								ParseHeartbeat(messageObject);
								break;
							case 10: // Hello
								ParseHello(messageObject);
								Identify();
								break;
							case 11: // Heartbeat Acknowledge
								_heartbeatAcknowledged = true;
								break;
						}
					}
				}
			}
		}

		private async void Identify()
		{
			JObject properties = new()
			{
				{ "$os", "linux" },
				{ "$browser", "none" },
				{ "$device", "none" }
			};

			JObject d = new()
			{
				{ "token", _settings.AccessToken },
				{ "properties", properties }
			};

			JObject identifyObject = new()
			{
				{ "op", 2 },
				{ "d", d },
				{ "intents", 0 }
			};

			await _gateway.SendAsync(Encoding.ASCII.GetBytes(identifyObject.ToString()), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
		}

		private void ParseHello(JObject helloObject)
		{
			lock (HeartbeatLock)
			{
				int heartbeatTime = helloObject["d"]!["heartbeat_interval"]!.Value<int>();

				_heartbeatTimer = new Timer(_ => SendHeartbeat(), null, heartbeatTime, heartbeatTime);
				_heartbeatAcknowledged = true;
			}
		}

		private void ParseHeartbeat(JObject heartbeatObject)
		{
			_sequenceNumber = heartbeatObject["d"]!.Value<int>();
		}

		private async void SendHeartbeat()
		{
			bool acknowledged;

			lock (HeartbeatLock)
			{
				acknowledged = _heartbeatAcknowledged;
			}

			if (!acknowledged)
			{
				await ShutDown(WebSocketCloseStatus.ProtocolError, "Did not receive heartbeat acknowledgement from server.");
				await ConnectWebsocket();
				return;
			}

			lock (HeartbeatLock)
			{ 
				_heartbeatAcknowledged = false;
			}

			JObject heartbeatObject = new() { { "op", 1 } };

			if (_sequenceNumber == -1)
			{
				heartbeatObject.Add("d", null);
			}
			else
			{
				heartbeatObject.Add("d", _sequenceNumber);
			}

			await _gateway.SendAsync(Encoding.ASCII.GetBytes(heartbeatObject.ToString()), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
		}

		public async Task ShutDown(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string closureMessage = "Shutting down.")
		{
			if (_gateway.State != WebSocketState.Closed && _gateway.State != WebSocketState.CloseReceived && _gateway.State != WebSocketState.CloseSent)
			{
				Console.WriteLine("Shutdown signal received, closing gateway.");
				await _gateway.CloseAsync(closeStatus, closureMessage, _cancellationTokenSource.Token);

				System.Diagnostics.Stopwatch stopwatch = new();

				stopwatch.Start();

				// Give it a few seconds.
				while (_gateway.State != WebSocketState.Closed && stopwatch.ElapsedMilliseconds < 10000)
				{
				}

				stopwatch.Stop();
			}
		}

		public async Task Post(IPostable post)
		{
			var discordMessage = new DiscordMessage(post);

			HttpContent messageContent = new StringContent(JsonConvert.SerializeObject(discordMessage), Encoding.UTF8, "application/json");

			using HttpClient httpClient = _httpClientFactory.CreateClient(HttpClients.Discord);
			httpClient.SetBotToken(_settings.AccessToken);
			string channel = post.Type == PostType.Administrative
				? _settings.PrivateChannelId
				: _settings.PublicChannelId;

			var response = await httpClient.PostAsync($"channels/{channel}/messages", messageContent, _cancellationTokenSource.Token);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Discord.");
				_logger.LogError(await response.Content.ReadAsStringAsync());
			}
		}

		public void Dispose()
		{
			_gateway.Dispose();
			_cancellationTokenSource.Dispose();
			_heartbeatTimer?.Dispose();
		}
	}
}
