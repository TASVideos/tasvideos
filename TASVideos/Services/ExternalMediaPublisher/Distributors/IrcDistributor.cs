using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TASVideos.Extensions;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class IrcDistributor : IPostDistributor
	{
		private static readonly object Sync = new object();
		private static IrcBot _bot;

		public IrcDistributor(
			IOptions<AppSettings> settings,
			ILogger<IrcDistributor> logger)
		{
			if (string.IsNullOrWhiteSpace(settings.Value.Irc.Password))
			{
				logger.Log(LogLevel.Warning, "Irc bot password not provided. Bot initialization skipped");
				return;
			}

			lock (Sync)
			{
				if (_bot == null)
				{
					_bot = new IrcBot(settings.Value.Irc);
				}
			}
		}

		public IEnumerable<PostType> Types => new[] { PostType.General, PostType.Announcement };

		public void Post(IPostable post)
		{
			var s = $"{post.Title.CapAndEllipse(100)} {post.Body.CapAndEllipse(100)} {post.Link}";
			_bot.AddMessage(s);
		}

		private class IrcBot
		{
			private readonly AppSettings.IrcConnection _settings;
			private readonly ConcurrentQueue<string> _work = new ConcurrentQueue<string>();

			public IrcBot(AppSettings.IrcConnection settings)
			{
				_settings = settings;
				Loop();
			}

			public void AddMessage(string item)
			{
				_work.Enqueue(item);
			}

			private async Task ConnectToServer()
			{
				using (var irc = new TcpClient(_settings.Server, _settings.Port))
				using (var stream = irc.GetStream())
				using (var reader = new StreamReader(stream))
				using (var writer = new StreamWriter(stream))
				{
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
							var inputLine = await reader.ReadLineAsync();
							Console.WriteLine("<- " + inputLine);

							// split the lines sent from the server by spaces (seems to be the easiest way to parse them)
							string[] splitInput = inputLine.Split(new[] { ' ' });

							if (splitInput[0] == "PING")
							{
								string pongReply = splitInput[1];
								writer.WriteLine("PONG " + pongReply);
								writer.Flush();
							}

							switch (splitInput[1])
							{
								case "001":
									await writer.WriteLineAsync("JOIN " + _settings.Channel);
									await writer.FlushAsync();
									break;
							}
						}
						else if (_work.TryDequeue(out var workItem))
						{
							try
							{
								await writer.WriteLineAsync($"PRIVMSG {_settings.Channel} :{workItem}");
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
						Console.WriteLine(e.ToString());
						await Task.Delay(30000);
					}
				}
				// ReSharper disable once FunctionNeverReturns
			}
		}
	}
}
