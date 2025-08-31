namespace TASVideos.E2E.Tests.Configuration;

public class E2ESettings
{
	public string BaseUrl { get; set; } = "https://tasvideos.org";
	public string LocalUrl { get; set; } = "https://localhost:44385";
	public string Environment { get; set; } = "Disabled";
	public int ThrottleDelayMs { get; set; } = 2000;
	public int RequestTimeoutMs { get; set; } = 30000;
	public int MaxRetryAttempts { get; set; } = 3;
	public bool HeadlessMode { get; set; } = true;
	public int SlowMo { get; set; }

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
