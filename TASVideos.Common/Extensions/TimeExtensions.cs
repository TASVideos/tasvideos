using System;

namespace TASVideos.Extensions
{
	public static class TimeExtensions
	{
		public static string ToCondensedString(this TimeSpan timeSpan)
		{
			if (timeSpan.Days > 0)
			{
				return timeSpan.ToString("d\\.hh\\:mm\\:ss\\.ff");
			}

			if (timeSpan.Hours > 0)
			{
				return timeSpan.ToString("h\\:mm\\:ss\\.ff");
			}

			if (timeSpan.Minutes > 0)
			{
				return timeSpan.ToString("m\\:ss\\.ff");
			}

			return timeSpan.ToString("s\\.ff");
		}
	}
}
