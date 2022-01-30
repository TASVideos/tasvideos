using TASVideos.Core.Data;

namespace TASVideos.Core.Settings;

public class AppSettings
{
	public string BaseUrl { get; set; } = "";
	public bool EnableGzipCompression { get; set; }

	public CacheSetting CacheSettings { get; set; } = new();

	public Connections ConnectionStrings { get; set; } = new();

	public IrcConnection Irc { get; set; } = new();
	public DiscordConnection Discord { get; set; } = new();
	public TwitterConnection Twitter { get; set; } = new();
	public JwtSettings Jwt { get; set; } = new();
	public GoogleAuthSettings YouTube { get; set; } = new();
	public GmailAuthSettings Gmail { get; set; } = new();

	public string StartupStrategy { get; set; } = "";
	public bool UseSampleDatabase { get; set; }

	// Minimum number of hours before a judge can set a submission to accepted/rejected
	public int MinimumHoursBeforeJudgment { get; set; }

	public class IrcConnection
	{
		public string Server { get; set; } = "";
		public string Channel { get; set; } = "";
		public string SecureChannel { get; set; } = "";
		public int Port { get; set; }
		public string Nick { get; set; } = "";
		public string Password { get; set; } = "";

		public bool IsEnabled() => !string.IsNullOrWhiteSpace(Server)
			&& !string.IsNullOrWhiteSpace(Channel)
			&& Port > 0
			&& !string.IsNullOrWhiteSpace(Nick)
			&& !string.IsNullOrWhiteSpace(Password);

		public bool IsSecureChannelEnabled() => IsEnabled() && !string.IsNullOrWhiteSpace(SecureChannel);
	}

	public class DiscordConnection
	{
		public string AccessToken { get; set; } = "";
		public string PublicChannelId { get; set; } = "";
		public string PrivateChannelId { get; set; } = "";

		public bool IsEnabled() => !string.IsNullOrWhiteSpace(AccessToken)
			&& !string.IsNullOrWhiteSpace(PublicChannelId);

		public bool IsPrivateChannelEnabled() => IsEnabled()
			&& !string.IsNullOrWhiteSpace(PrivateChannelId);
	}

	public class TwitterConnection
	{
		public string ApiBase { get; set; } = "";
		public string ConsumerKey { get; set; } = "";
		public string ConsumerSecret { get; set; } = "";
		public string AccessToken { get; set; } = "";
		public string TokenSecret { get; set; } = "";

		public bool IsEnabled() => !string.IsNullOrWhiteSpace(ApiBase)
			&& !string.IsNullOrWhiteSpace(ConsumerKey)
			&& !string.IsNullOrWhiteSpace(ConsumerSecret)
			&& !string.IsNullOrWhiteSpace(AccessToken)
			&& !string.IsNullOrWhiteSpace(TokenSecret);
	}

	public class CacheSetting
	{
		public string CacheType { get; set; } = "NoCache";
		public int CacheDurationInSeconds { get; set; }
		public string ConnectionString { get; set; } = "";
	}

	public class Connections
	{
		public string PostgresConnection { get; set; } = "";
		public string PostgresSampleDataConnection { get; set; } = "";
	}

	public class JwtSettings
	{
		public string SecretKey { get; set; } = "";
		public int ExpiresInMinutes { get; set; }
	}

	public class GoogleAuthSettings
	{
		public string ClientId { get; set; } = "";
		public string ClientSecret { get; set; } = "";
		public string RefreshToken { get; set; } = "";

		public virtual bool IsEnabled() =>
			!string.IsNullOrWhiteSpace(ClientId)
			&& !string.IsNullOrWhiteSpace(ClientSecret)
			&& !string.IsNullOrWhiteSpace(RefreshToken);
	}

	public class GmailAuthSettings : GoogleAuthSettings
	{
		public string From { get; set; } = "";

		public override bool IsEnabled()
		{
			return !string.IsNullOrWhiteSpace(From) && base.IsEnabled();
		}
	}
}

public static class AppSettingsExtensions
{
	public static StartupStrategy GetStartupStrategy(this AppSettings settings)
	{
		var strategy = settings.StartupStrategy;
		if (!string.IsNullOrWhiteSpace(settings.StartupStrategy))
		{
			var result = Enum.TryParse(typeof(StartupStrategy), strategy, true, out object? strategyObj);

			if (result)
			{
				return (StartupStrategy)(strategyObj ?? StartupStrategy.Minimal);
			}
		}

		return StartupStrategy.Minimal;
	}
}
