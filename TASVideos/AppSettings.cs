namespace TASVideos
{
	public class AppSettings
	{
		public bool EnableGzipCompression { get; set; }

		public CacheSetting CacheSettings { get; set; } = new CacheSetting();

		public Connections ConnectionStrings { get; set; } = new Connections();

		public IrcConnection GeneralIrc { get; set;} = new IrcConnection();

		public class IrcConnection
		{
			public string Server { get; set; }
			public string Channel { get; set; }
			public int Port { get; set; }
			public string BotName { get; set; }
		}

		public class CacheSetting
		{
			public string CacheType { get; set; }
			public int CacheDurationInSeconds { get; set; }
		}

		public class Connections
		{
			public string DefaultConnection { get; set; }
		}
	}
}
