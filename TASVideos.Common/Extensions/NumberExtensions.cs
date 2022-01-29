﻿using System;

namespace TASVideos.Extensions;

public static class NumberExtensions
{
	/// <summary>
	///   Limit a value to a certain range. When the value is smaller/bigger than the range, snap it to the range border.
	/// </summary>
	/// <typeparam name = "T">The type of the value to limit.</typeparam>
	/// <param name = "source">The source for this extension method.</param>
	/// <param name = "start">The start of the interval, included in the interval.</param>
	/// <param name = "end">The end of the interval, included in the interval.</param>
	public static T Clamp<T>(this T source, T start, T end)
		where T : IComparable
	{
		bool isReversed = start.CompareTo(end) > 0;
		T smallest = isReversed ? end : start;
		T biggest = isReversed ? start : end;

		return source.CompareTo(smallest) < 0
			? smallest
			: source.CompareTo(biggest) > 0
				? biggest
				: source;
	}

	public static decimal ToPercent(this int val, int total, int precision = 1)
	{
		if (total == 0)
		{
			return 0;
		}

		var p = val / (decimal)total;
		return Math.Round(p * 100, precision);
	}

	/// <summary>
	/// Returns the number of bytes as a format file size string
	/// such as 1 KB, 1 MB, 1GB.
	/// </summary>
	public static string ToSizeString(this int byteCount)
	{
		if (byteCount > 1_000_000_000)
		{
			return $"{byteCount / 1_000_000_000f:f2} GB";
		}

		if (byteCount > 1_000_000)
		{
			return $"{byteCount / 1_000_000:f2} MB";
		}

		if (byteCount > 1_000)
		{
			return $"{byteCount / 1_000f:f2} KB";
		}

		return $"{byteCount} bytes";
	}
}
