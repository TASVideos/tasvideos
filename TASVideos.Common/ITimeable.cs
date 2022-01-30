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

		return TimeSpan.FromMilliseconds(Math.Round(timeable.Frames / timeable.FrameRate * 100, MidpointRounding.AwayFromZero) * 10);
	}

	public static string ToStringWithOptionalDaysAndHours(this TimeSpan timeSpan)
	{
		if (timeSpan.Days >= 1)
		{
			return timeSpan.ToString(@"d\:hh\:mm\:ss\.ff");
		}

		if (timeSpan.Hours >= 1)
		{
			return timeSpan.ToString(@"h\:mm\:ss\.ff");
		}

		return timeSpan.ToString(@"mm\:ss\.ff");
	}

	public static string ToRelativeString(this TimeSpan relativeTime)
	{
		if (relativeTime.TotalSeconds < 5)
		{
			return "Now";
		}
		else if (relativeTime.TotalSeconds < 60)
		{
			return $"{relativeTime.Seconds} seconds ago";
		}
		else if (relativeTime.TotalMinutes < 2)
		{
			return "1 minute ago";
		}
		else if (relativeTime.TotalMinutes < 60)
		{
			return $"{relativeTime.Minutes} minutes ago";
		}
		else if (relativeTime.TotalHours < 2)
		{
			return "1 hour ago";
		}
		else if (relativeTime.TotalHours < 24)
		{
			return $"{relativeTime.Hours} hours ago";
		}
		else if (relativeTime.TotalDays < 2)
		{
			return "1 day ago";
		}
		else
		{
			return $"{relativeTime.Days} days ago";
		}
	}
}
