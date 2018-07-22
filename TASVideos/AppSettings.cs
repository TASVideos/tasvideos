namespace TASVideos
{
	public class AppSettings
	{
		public bool EnableGzipCompression { get; set; }

		public CacheSetting CacheSettings { get; set; } = new CacheSetting();

		public Connections ConnectionStrings { get; set; } = new Connections();

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
