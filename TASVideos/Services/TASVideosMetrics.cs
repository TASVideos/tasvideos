using System.Diagnostics.Metrics;

namespace TASVideos.Services;

public interface ITASVideosMetrics
{
	public void AddUserAgent(string? userAgent);
}

public class NullMetrics : ITASVideosMetrics
{
	public void AddUserAgent(string? userAgent)
	{
	}
}

public class TASVideosMetrics : ITASVideosMetrics
{
	private readonly Counter<long> _userAgentCounter;

	public TASVideosMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create("TASVideos");
		_userAgentCounter = meter.CreateCounter<long>("tasvideos.useragent.count");
	}

	public void AddUserAgent(string? userAgent)
	{
		_userAgentCounter.Add(1, new KeyValuePair<string, object?>("user_agent", userAgent));
	}
}
