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
			return TimeSpan.FromMilliseconds(Math.Round(timeable.Frames / timeable.FrameRate * 100) * 10);
		}
	}
}
