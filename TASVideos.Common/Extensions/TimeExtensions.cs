namespace TASVideos.Extensions;

public static class TimeExtensions
{
	public static int DaysAgo(this DateTime startDate)
	{
		TimeSpan elapsed = DateTime.UtcNow.Subtract(startDate);
		double daysAgo = elapsed.TotalDays;
		return (int)Math.Round(daysAgo);
	}

	public static long UnixTimestamp(this DateTime dateTime)
	{
		return ((DateTimeOffset)DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)).ToUnixTimeSeconds();
	}
}
