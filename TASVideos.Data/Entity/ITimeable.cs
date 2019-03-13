using System;

namespace TASVideos.Data.Entity
{
	public interface ITimeable
	{
		double FrameRate { get; }
		int Frames { get; } 
	}

	public static class TimeableExtensions
	{
		public static TimeSpan Time(this ITimeable timeable)
		{
			int seconds = (int)(timeable.Frames / timeable.FrameRate);
			double fractionalSeconds = (timeable.Frames / timeable.FrameRate) - seconds;
			int milliseconds = (int)(Math.Round(fractionalSeconds, 2) * 1000);
			var timespan = new TimeSpan(0, 0, 0, seconds, milliseconds);
			return timespan;
		}
	}
}
