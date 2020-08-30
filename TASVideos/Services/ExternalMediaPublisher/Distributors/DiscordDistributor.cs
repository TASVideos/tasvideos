using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TASVideos.Api.Requests;
using TASVideos.Pages.Profile;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class DiscordDistributor : IPostDistributor
	{
		const int BUFFER_SIZE = 65535;

		ILogger _logger;

		private readonly AppSettings.DiscordConnection _settings;

		private bool _initialized = false;

		// TODO: register with HttpClient factor and DI
		// https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
		static readonly HttpClient _httpClient = new HttpClient();

		internal static ClientWebSocket? _gateway;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		private int _sequenceNumber = -1;
		private bool _heartbeatAcknowledged = false;

		Timer? _heartbeatTimer;

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public DiscordDistributor (IOptions<AppSettings> appSettings, ILogger<DiscordDistributor> logger)
		{
			_logger = logger;
			_settings = appSettings.Value.Discord;

			if (string.IsNullOrWhiteSpace(appSettings.Value.Discord.AccessToken))
			{
				logger.Log(LogLevel.Warning, "Discord bot access key not provided. Bot initialization skipped");
				return;
			}

			_initialized = true;
			ConnectWebsocket();
		}

		private async void ConnectWebsocket ()
		{
			var receiveBuffer = WebSocket.CreateClientBuffer(BUFFER_SIZE, 4096);
			string message;

			Uri gatewayUri = new Uri("wss://gateway.discord.gg/?v=6&encoding=json");
			_gateway = new ClientWebSocket();
			CancellationToken cancellationToken = cancellationTokenSource.Token;

			await _gateway.ConnectAsync(gatewayUri, cancellationToken);

			while (_gateway.State == WebSocketState.Open || _gateway.State == WebSocketState.Connecting)
			{
				WebSocketReceiveResult result = await _gateway.ReceiveAsync(receiveBuffer, cancellationToken);

				message = Encoding.ASCII.GetString(receiveBuffer.Array!, receiveBuffer.Offset, result.Count);

				while (!result.EndOfMessage)
				{
					result = await _gateway.ReceiveAsync(receiveBuffer, cancellationToken);
					message += Encoding.ASCII.GetString(receiveBuffer.Array!, receiveBuffer.Offset, result.Count);
				}

				HandleMessage(message);
			}
		}

		private void HandleMessage (string message)
		{
			if (!String.IsNullOrEmpty(message))
			{
				Console.WriteLine($"-> Discord: {message}");

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
							this._heartbeatAcknowledged = true;
							break;
					}
				}
			}
		}

		private async void Identify()
		{
			JObject identifyObject = new JObject();
			identifyObject.Add("op", 2);

			JObject d = new JObject();
			d.Add("token", _settings.AccessToken);

			JObject properties = new JObject();
			properties.Add("$os", "linux");
			properties.Add("$browser", "none");
			properties.Add("$device", "none");

			d.Add("properties", properties);

			identifyObject.Add("d", d);

			await _gateway!.SendAsync(Encoding.ASCII.GetBytes(identifyObject.ToString()), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
		}

		private void ParseHello(JObject helloObject)
		{
#pragma warning disable 8604
			int heartbeatTime = helloObject["d"]!["heartbeat_interval"]!.Value<int>();
#pragma warning restore 8604

			_heartbeatTimer = new Timer(callback => SendHeartbeat(), null, heartbeatTime, heartbeatTime);
		}

		private void ParseHeartbeat (JObject heartbeatObject)
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

			JObject heartbeatObject = new JObject();
			heartbeatObject.Add("op", 1);

			if (_sequenceNumber == -1)
			{
				heartbeatObject.Add("d", null);
			}
			else
			{
				heartbeatObject.Add("d", _sequenceNumber);
			}

			await _gateway!.SendAsync(Encoding.ASCII.GetBytes(heartbeatObject.ToString()), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
		}

		public async Task ShutDown (WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string closureMessage = "Shutting down.")
		{
			await _gateway!.CloseAsync(closeStatus, closureMessage, cancellationTokenSource.Token);
		}

		public async void Post (IPostable post)
		{
			if (!_initialized)
			{
				return;
			}

			DiscordMessage discordMessage = new DiscordMessage(post.Title, post.Body, post.Link);

			HttpRequestMessage apiRequest = new HttpRequestMessage();
			apiRequest.Method = HttpMethod.Post;
			apiRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _settings.AccessToken);
			apiRequest.Content = new StringContent(discordMessage.Serialize(), Encoding.UTF8, "application/json");
			apiRequest.RequestUri = new Uri($"{_settings.ApiBase}/channels/{_settings.ChannelId}/messages");

			var response = await _httpClient!.SendAsync(apiRequest).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Discord.");
				_logger.LogError(await response.Content.ReadAsStringAsync());
			}
		}
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

			serializedMessage.Add("content", $"New post{(String.IsNullOrEmpty(PostUser) ? "." : $" from {PostUser}.")}");

			embedObject.Add("title", PostTitle);
			embedObject.Add("description", PostBody);
			embedObject.Add("url", PostUrl);

			serializedMessage.Add("embed", embedObject);

			return serializedMessage.ToString();
		}
	}
}
