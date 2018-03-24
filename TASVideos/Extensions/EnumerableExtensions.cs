using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Extensions
{
    public static class EnumerableExtensions
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

		/// <summary>
		/// Returns the first half of the given collection,
		/// in the case of odd numbers, the odd item is considered in the first half
		/// </summary>
		public static IEnumerable<T> FirstHalf<T>(this IEnumerable<T> source)
		{
			int half = (int)Math.Ceiling(source.Count() / 2.0);
			return source.Take(half);
		}

		/// <summary>
		/// Returns the second half of the given collection,
		/// in the case of odd numbers, the odd item is considered in the first half
		/// </summary>
		public static IEnumerable<T> SecondHalf<T>(this IEnumerable<T> source)
		{
			int half = (int)Math.Ceiling(source.Count() / 2.0);
			return source.Skip(half);
		}
	}
}
