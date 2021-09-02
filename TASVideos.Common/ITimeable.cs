﻿using System;

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
	}
}
