namespace TASVideos.Extensions;

public static class TimeExtensions
{
	public static int DaysAgo(this DateTime startDate)
	{
		TimeSpan elapsed = DateTime.UtcNow.Subtract(startDate);
		double daysAgo = elapsed.TotalDays;
		return (int)Math.Round(daysAgo);
	}
}
