namespace TASVideos.Extensions;

public static class NumberExtensions
{
	/// <summary>
	/// Limit a value to a certain range. When the value is smaller/bigger than the range, snap it to the range border.
	/// </summary>
	/// <typeparam name = "T">The type of the value to limit.</typeparam>
	/// <param name = "source">The source for this extension method.</param>
	/// <param name = "start">The start of the interval, included in the interval.</param>
	/// <param name = "end">The end of the interval, included in the interval.</param>
	public static T Clamp<T>(this T source, T start, T end)
		where T : IComparable
	{
		var isReversed = start.CompareTo(end) > 0;
		var smallest = isReversed ? end : start;
		var biggest = isReversed ? start : end;

		return source.CompareTo(smallest) < 0
			? smallest
			: source.CompareTo(biggest) > 0
				? biggest
				: source;
	}

	extension(int val)
	{
		public decimal ToPercent(int total, int precision = 1)
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
		public string ToSizeString()
			=> val switch
			{
				> 1_000_000_000 => $"{val / 1_000_000_000f:f2} GB",
				> 1_000_000 => $"{val / 1_000_000:f2} MB",
				> 1_000 => $"{val / 1_000f:f2} KB",
				_ => $"{val} bytes"
			};
	}

	extension(double overallRating)
	{
		public string ToOverallRatingString()
			=> Math.Round(overallRating, 2, MidpointRounding.AwayFromZero).ToString();

		/// <summary>Displays a number between 0 and 1 as a percentage. It rounds down so that 0.999999 is not shown as 100%</summary>
		public string ToPercentage(int decimalPlaces = 0)
		{
			if (double.IsNaN(overallRating) || double.IsInfinity(overallRating))
			{
				return "n/a";
			}

			return (Math.Floor(overallRating * 100 * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces)).ToString($"F{decimalPlaces}") + "%";
		}
	}
}
