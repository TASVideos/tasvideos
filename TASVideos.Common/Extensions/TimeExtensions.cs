namespace TASVideos.Extensions;

public static class TimeExtensions
{
	extension(DateTime startDate)
	{
		public int DaysAgo()
		{
			var elapsed = DateTime.UtcNow.Subtract(startDate);
			var daysAgo = elapsed.TotalDays;
			return (int)Math.Round(daysAgo);
		}

		public long UnixTimestamp() => ((DateTimeOffset)DateTime.SpecifyKind(startDate, DateTimeKind.Utc)).ToUnixTimeSeconds();
	}
}
