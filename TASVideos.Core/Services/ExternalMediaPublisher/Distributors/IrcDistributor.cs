using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Settings;
using TASVideos.Extensions;

// ReSharper disable FunctionNeverReturns
namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors
{
	public class IrcDistributor : IPostDistributor
	{
		private static readonly object Sync = new ();
		private static IrcBot? _bot;
		private readonly AppSettings.IrcConnection _settings;

		public IrcDistributor(
			AppSettings settings,
			ILogger<IrcDistributor> logger)
		{
			_settings = settings.Irc;

			if (string.IsNullOrWhiteSpace(settings.Irc.Password))
			{
				logger.Log(LogLevel.Warning, "Irc bot password not provided. Bot initialization skipped");
				return;
			}

			lock (Sync)
			{
				_bot ??= new IrcBot(_settings, logger);
			}
		}

		public IEnumerable<PostType> Types => new[] { PostType.Administrative, PostType.General, PostType.Announcement };

		public async Task Post(IPostable post)
		{
			// If proper credentials were not provided, the bot was never initialized
			if (_bot == null)
			{
				return;
			}

			string channel = post.Type == PostType.Administrative
				? _settings.SecureChannel
				: _settings.Channel;

			string content;
			if (string.IsNullOrWhiteSpace(post.Body))
			{
				content = post.Title.CapAndEllipse(150 + 200 + 3);
			}
			else
			{
				content = $"{post.Title.CapAndEllipse(150)} ({post.Body.CapAndEllipse(200)})";
			}

			var s = $"{content}{(string.IsNullOrWhiteSpace(post.Link) ? "" : $" {post.Link}")}";
			await Task.Run(() => _bot.AddMessage(channel, s));
		}

		private class IrcBot
		{
			private readonly AppSettings.IrcConnection _settings;
			private readonly ILogger _logger;
			private readonly ConcurrentQueue<string> _work = new ();

			public IrcBot(AppSettings.IrcConnection settings, ILogger logger)
			{
				_settings = settings;
				_logger = logger;
				#pragma warning disable CS4014
				Loop();
				#pragma warning restore CS4014
			}

			public void AddMessage(string channel, string item)
			{
				_work.Enqueue($"PRIVMSG {channel} :{item}".NewlinesToSpaces());
			}

			private async Task ConnectToServer()
			{
				using var irc = new TcpClient(_settings.Server, _settings.Port);
				await using var stream = irc.GetStream();
				using var reader = new StreamReader(stream);
				await using var writer = new StreamWriter(stream);
				await writer.WriteLineAsync($"NICK {_settings.Nick}");
				await writer.FlushAsync();

				await writer.WriteLineAsync($"USER {_settings.Nick} 0 * :This is TASVideos bot in development");
				await writer.FlushAsync();

				await writer.WriteLineAsync($"PRIVMSG NickServ :identify {_settings.Nick} {_settings.Password}");
				await writer.FlushAsync();

				while (true)
				{
					if (stream.DataAvailable)
					{
						var inputLine = await reader.ReadLineAsync() ?? "";
						_logger.LogDebug("<- " + inputLine);

						// split the lines sent from the server by spaces (seems to be the easiest way to parse them)
						string[] splitInput = inputLine.Split(new[] { ' ' });

						if (splitInput[0] == "PING")
						{
							string pongReply = splitInput[1];
							await writer.WriteLineAsync("PONG " + pongReply);
							await writer.FlushAsync();
						}

						switch (splitInput[1])
						{
							case "001":
								var channels = $"{_settings.Channel},{_settings.SecureChannel}";
								await writer.WriteLineAsync("JOIN " + channels);
								await writer.FlushAsync();
								break;
						}
					}
					else if (_work.TryDequeue(out var workItem))
					{
						try
						{
							await writer.WriteLineAsync(workItem);
							await writer.FlushAsync();
						}
						catch
						{
							_work.Enqueue(workItem);
							throw;
						}

						await Task.Delay(10000);
					}
					else
					{
						await Task.Delay(1000);
					}
				}
			}

			private async Task Loop()
			{
				while (true)
				{
					try
					{
						await ConnectToServer();
					}
					catch (Exception e)
					{
						// shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
						_logger.LogWarning(e.ToString());
						await Task.Delay(30000);
					}
				}
			}
		}
	}
}
