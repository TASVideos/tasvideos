namespace TASVideos.E2E.Tests.Configuration;

public class E2ESettings
{
	public string BaseUrl { get; init; } = "https://tasvideos.org";
	public string LocalUrl { get; init; } = "https://localhost:44385";
	public string Environment { get; init; } = "Disabled";
	public int ThrottleDelayMs { get; init; } = 2000;
	public int RequestTimeoutMs { get; init; } = 30000;
	public int MaxRetryAttempts { get; init; } = 3;
	public bool HeadlessMode { get; init; } = true;
	public int SlowMo { get; init; }

	public string GetTestUrl()
		=> Environment.Equals("Local", StringComparison.OrdinalIgnoreCase)
			? LocalUrl
			: BaseUrl;

	public bool IsProductionEnvironment()
		=> Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

	public bool IsEnabled()
		=> Environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
		|| Environment.Equals("Local", StringComparison.OrdinalIgnoreCase);
}
