using System;

namespace TASVideos.Common
{
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
			int seconds = (int)(timeable.Frames / timeable.FrameRate);
			double fractionalSeconds = (timeable.Frames / timeable.FrameRate) - seconds;
			int milliseconds = (int)(Math.Round(fractionalSeconds, 2) * 1000);
			var timespan = new TimeSpan(0, 0, 0, seconds, milliseconds);
			return timespan;
		}
	}
}
