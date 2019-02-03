using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TASVideoAgent
{
	public class Tva
	{
		private readonly Settings _settings = new Settings();

		private readonly ConcurrentQueue<string> _work = new ConcurrentQueue<string>();

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
						string[] splitInput = inputLine.Split(new char[] { ' ' });

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
							var toSend = workItem;
							if (toSend.Length > 100)
							{
								toSend = toSend.Substring(0, 97) + "...";
							}
							await writer.WriteLineAsync($"PRIVMSG {_settings.Channel} :{toSend}");
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

		public async Task Loop()
		{
			bool retry;
			for (var retryCount = -1; retryCount < _settings.MaxRetries; retryCount++)
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
		}

		public void AddMessage(string item)
		{
			_work.Enqueue(item);
		}
	}
}
