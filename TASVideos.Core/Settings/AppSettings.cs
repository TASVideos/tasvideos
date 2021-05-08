namespace TASVideos.Core.Settings
{
	public class AppSettings
	{
		public string BaseUrl { get; set; } = "";
		public bool EnableGzipCompression { get; set; }

		public CacheSetting CacheSettings { get; set; } = new ();

		public Connections ConnectionStrings { get; set; } = new ();

		public IrcConnection Irc { get; set; } = new ();
		public DiscordConnection Discord { get; set; } = new ();
		public TwitterConnection Twitter { get; set; } = new ();
		public JwtSettings Jwt { get; set; } = new ();

		public string StartupStrategy { get; set; } = "";

		public string SendGridKey { get; set; } = "";
		public string SendGridFrom { get; set; } = "";
		public bool UsePostgres { get; set; } = true;

		public class IrcConnection
		{
			public string Server { get; set; } = "";
			public string Channel { get; set; } = "";
			public string SecureChannel { get; set; } = "";
			public int Port { get; set; }
			public string Nick { get; set; } = "";
			public string Password { get; set; } = "";
		}

		public class DiscordConnection
		{
			public string AccessToken { get; set; } = "";
			public string ApiBase { get; set; } = "";
			public string AuthEndpoint { get; set; } = "";
			public string PublicChannelId { get; set; } = "";
			public string PrivateChannelId { get; set; } = "";
			public string Scopes { get; set; } = "bot";
		}

		public class TwitterConnection
		{
			public string ApiBase { get; set; } = "";
			public string ConsumerKey { get; set; } = "";
			public string ConsumerSecret { get; set; } = "";
			public string AccessToken { get; set; } = "";
			public string TokenSecret { get; set; } = "";
		}

		public class CacheSetting
		{
			public string CacheType { get; set; } = "NoCache";
			public int CacheDurationInSeconds { get; set; }
		}

		public class Connections
		{
			public string DefaultConnection { get; set; } = "";
			public string PostgresConnection { get; set; } = "";
			public string? LegacySiteConnection { get; set; }
			public string? LegacyForumConnection { get; set; }
		}

		public class JwtSettings
		{
			public string SecretKey { get; set; } = "";
			public int ExpiresInMinutes { get; set; }
		}
	}
}
