using System;
using System.Linq;

namespace TASVideos.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Truncates the end of a string to the given character <see cref="limit"/> if the 
		/// strength exceeds this limit, else the string itself is returned.
		/// If length exceeds limit, ellipses will be added to the result
		/// </summary>
		public static string CapAndEllipse(this string str, int limit)
		{
			if (str == null)
			{
				return null;
			}

			if (limit < 0)
			{
				throw new ArgumentException($"{nameof(limit)} cannot be less than zero");
			}

			if (limit == 0)
			{
				return "";
			}

			if (str.Length <= limit)
			{
				return str;
			}

			if (limit <= 3)
			{
				return new string(Enumerable.Repeat('.', limit).ToArray());
			}

			return str.Substring(0, limit - 3) + "...";
		}
	}
}
