namespace TASVideos
{
	public class AppSettings
	{
		public CacheSetting CacheSettings { get; set; } = new CacheSetting();

		public class CacheSetting
		{
			public string CacheType { get; set; }
			public int CacheDurationInSeconds { get; set; }
		}
	}
}
