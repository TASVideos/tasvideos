using TASVideos.Core.Data;

namespace TASVideos.Core.Settings;

public class AppSettings
{
	public string BaseUrl { get; set; } = "";
	public bool EnableGzipCompression { get; set; }

	public CacheSetting CacheSettings { get; set; } = new();

	public Connections ConnectionStrings { get; set; } = new();

	public SubmissionRateLimit SubmissionRate { get; set; } = new();

	public IrcConnection Irc { get; set; } = new();
	public DiscordConnection Discord { get; set; } = new();
	public BlueskyConnection Bluesky { get; set; } = new();

	public JwtSettings Jwt { get; set; } = new();
	public GoogleAuthSettings YouTube { get; set; } = new();
	public EmailBasicAuthSettings Email { get; set; } = new();

	public string StartupStrategy { get; set; } = "";
	public bool UseSampleDatabase { get; set; }

	// Minimum number of hours before a judge can set a submission to accepted/rejected
	public int MinimumHoursBeforeJudgment { get; set; }

	public ReCaptchaSettings ReCaptcha { get; set; } = new();

	public bool EnableMetrics { get; set; }

	// User is only allowed to submit X submissions in Y days
	public class SubmissionRateLimit
	{
		public int Submissions { get; set; }
		public int Days { get; set; }
	}

	public class IrcConnection : DistributorConnection
	{
		public string Server { get; set; } = "";
		public string Channel { get; set; } = "";
		public string SecureChannel { get; set; } = "";
		public int Port { get; set; }
		public string Nick { get; set; } = "";
		public string Password { get; set; } = "";

		public bool IsEnabled() => Disable != true
			&& !string.IsNullOrWhiteSpace(Server)
			&& !string.IsNullOrWhiteSpace(Channel)
			&& Port > 0
			&& !string.IsNullOrWhiteSpace(Nick)
			&& !string.IsNullOrWhiteSpace(Password);

		public bool IsSecureChannelEnabled() => IsEnabled() && !string.IsNullOrWhiteSpace(SecureChannel);
	}

	public class DiscordConnection : DistributorConnection
	{
		public string AccessToken { get; set; } = "";
		public string PublicChannelId { get; set; } = "";
		public string PublicTasChannelId { get; set; } = "";
		public string PublicGameChannelId { get; set; } = "";
		public string PrivateChannelId { get; set; } = "";
		public string PrivateUserChannelId { get; set; } = "";

		public bool IsEnabled() => Disable != true
			&& !string.IsNullOrWhiteSpace(AccessToken)
			&& !string.IsNullOrWhiteSpace(PublicChannelId)
			&& !string.IsNullOrWhiteSpace(PublicTasChannelId)
			&& !string.IsNullOrWhiteSpace(PublicGameChannelId);

		public bool IsPrivateChannelEnabled() => IsEnabled()
			&& !string.IsNullOrWhiteSpace(PrivateChannelId)
			&& !string.IsNullOrWhiteSpace(PrivateUserChannelId);
	}

	public class BlueskyConnection : DistributorConnection
	{
		public string Identifier { get; set; } = "";
		public string Password { get; set; } = "";

		public bool IsEnabled() => Disable != true
			&& !string.IsNullOrWhiteSpace(Identifier)
			&& !string.IsNullOrWhiteSpace(Password);
	}

	public class DistributorConnection
	{
		public bool? Disable { get; set; }
	}

	public class CacheSetting
	{
		public string CacheType { get; set; } = "NoCache";
		public int CacheDurationInSeconds { get; set; }
		public TimeSpan CacheDuration => TimeSpan.FromSeconds(CacheDurationInSeconds);
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

	public class EmailBasicAuthSettings
	{
		public string SmtpServer { get; set; } = "";
		public int SmtpServerPort { get; set; } = 587;
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";

		public bool IsEnabled()
		{
			return !string.IsNullOrWhiteSpace(Email)
				&& !string.IsNullOrWhiteSpace(Password)
				&& !string.IsNullOrWhiteSpace(SmtpServer);
		}
	}

	public class ReCaptchaSettings
	{
		public string Version { get; set; } = "";
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
