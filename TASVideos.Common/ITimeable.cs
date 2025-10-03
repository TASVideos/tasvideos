namespace TASVideos.Common;

public interface ITimeable
{
	double FrameRate { get; }
	int Frames { get; }
}

public class Timeable : ITimeable
{
	public double FrameRate { get; init; }
	public int Frames { get; init; }
}

public static class TimeableExtensions
{
	public static TimeSpan Time(this ITimeable timeable)
	{
		// Account for nonsense
		if (timeable.Frames == 0)
		{
			return TimeSpan.Zero;
		}

		if (timeable.FrameRate < 0.00001)
		{
			return TimeSpan.MaxValue;
		}

		return TimeSpan.FromMilliseconds(Math.Round(timeable.Frames / timeable.FrameRate * 1000, MidpointRounding.AwayFromZero));
	}

	public static string ToStringWithOptionalDaysAndHours(this TimeSpan timeSpan)
	{
		if (timeSpan.Days >= 1)
		{
			return timeSpan.ToString(@"d\:hh\:mm\:ss\.fff");
		}

		if (timeSpan.Hours >= 1)
		{
			return timeSpan.ToString(@"h\:mm\:ss\.fff");
		}

		if (timeSpan.TotalSeconds <= 0.001)
		{
			return "00:00.001";
		}

		return timeSpan.ToString(@"mm\:ss\.fff");
	}

	public static string ToRelativeString(this TimeSpan relativeTime)
	{
		return relativeTime.TotalSeconds switch
		{
			< 5 => "Now",
			< 60 => $"{relativeTime.Seconds} seconds ago",
			_ => relativeTime.TotalMinutes switch
			{
				< 2 => "1 minute ago",
				< 60 => $"{relativeTime.Minutes} minutes ago",
				_ => relativeTime.TotalHours switch
				{
					< 2 => "1 hour ago",
					< 24 => $"{relativeTime.Hours} hours ago",
					_ => relativeTime.TotalDays < 2
						? "1 day ago"
						: $"{relativeTime.Days} days ago"
				}
			}
		};
	}
}
