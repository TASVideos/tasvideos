using System;

using TASVideos.Data;

namespace TASVideos
{
	public class AppSettings
	{
		public bool EnableGzipCompression { get; set; }

		public CacheSetting CacheSettings { get; set; } = new CacheSetting();

		public Connections ConnectionStrings { get; set; } = new Connections();

		public IrcConnection Irc { get; set; } = new IrcConnection();
		public DiscordConnection Discord { get; set; } = new DiscordConnection();

		public string StartupStrategy { get; set; } = "";

		public string SendGridKey { get; set; } = "";
		public string SendGridFrom { get; set; } = "";

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

		public class CacheSetting
		{
			public string CacheType { get; set; } = "NoCache";
			public int CacheDurationInSeconds { get; set; }
		}

		public class Connections
		{
			public string DefaultConnection { get; set; } = "";
			public string? LegacySiteConnection { get; set; }
			public string? LegacyForumConnection { get; set; }
		}
	}

	public static class AppSettingsExtensions
	{
		public static DbInitializer.StartupStrategy StartupStrategy(this AppSettings settings)
		{
			var strategy = settings.StartupStrategy;
			if (!string.IsNullOrWhiteSpace(settings.StartupStrategy))
			{
				var result = Enum.TryParse(typeof(DbInitializer.StartupStrategy), strategy, true, out object? strategyObj);
			
				if (result)
				{
					return (DbInitializer.StartupStrategy)(strategyObj ?? DbInitializer.StartupStrategy.Minimal);
				}
			}

			return DbInitializer.StartupStrategy.Minimal;
		}
	}
}
