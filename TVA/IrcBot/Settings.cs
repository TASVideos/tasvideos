namespace TASVideoAgent
{
    public class Settings
    {
        public string Server { get; } = "irc.freenode.net";
        public int Port { get; } = 6667;
        public string Nick { get; } = "TASVideosAgentD";
        public string Channel { get; } = "#tasvideosdevirc";
        public string Password { get; } = "6yR4Bh2NErzFYpby";
        public int MaxRetries { get; } = 3;
    }
}
