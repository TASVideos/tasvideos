using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TASVideoAgent
{
    public class Tva
    {
        private readonly Settings _settings = new Settings();

        public async Task Start()
        {
            bool retry;
            var retryCount = 0;
            do
            {
                try
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

                        while (true)
                        {
                            string inputLine;
                            while ((inputLine = await reader.ReadLineAsync()) != null)
                            {
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
                        }
                    }
                }
                catch (Exception e)
                {
                    // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    retry = ++retryCount <= _settings.MaxRetries;
                }
            } while (retry);
        }
    }
}
