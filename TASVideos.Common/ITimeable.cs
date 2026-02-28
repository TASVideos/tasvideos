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

	extension(TimeSpan timeSpan)
	{
		public string ToStringWithOptionalDaysAndHours()
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

		public string ToRelativeString()
		{
			var isFuture = timeSpan.Ticks > 0;
			var duration = timeSpan.Duration();

			return duration.TotalSeconds switch
			{
				< 5 => "Now",
				< 60 => isFuture ? $"In {duration.Seconds} seconds" : $"{duration.Seconds} seconds ago",
				_ => duration.TotalMinutes switch
				{
					< 2 => isFuture ? "In 1 minute" : "1 minute ago",
					< 60 => isFuture ? $"In {duration.Minutes} minutes" : $"{duration.Minutes} minutes ago",
					_ => duration.TotalHours switch
					{
						< 2 => isFuture ? "In 1 hour" : "1 hour ago",
						< 24 => isFuture ? $"In {duration.Hours} hours" : $"{duration.Hours} hours ago",
						_ => duration.TotalDays < 2
							? isFuture ? "Tomorrow" : "Yesterday"
							: isFuture ? $"In {duration.Days} days" : $"{duration.Days} days ago"
					}
				}
			};
		}
	}
}
