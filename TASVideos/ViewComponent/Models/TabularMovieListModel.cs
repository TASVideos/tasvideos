using System;
using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class TabularMovieListSearchModel
	{
		public int Limit { get; set; } = 10;
		public IEnumerable<string> Tiers { get; set; } = new List<string>();
	}

	public class TabularMovieListResultModel
	{
		public int Id { get; set; }
		public DateTime CreateTimeStamp { get; set; }
		public TimeSpan Time { get; set; }

		public int? ObsoletedBy { get; set; }
		public TimeSpan? PreviousTime { get; set; }
		public int PreviousId { get; set; }

		public string Game { get; set; }
		public string Authors { get; set; }

		public ScreenshotFile Screenshot { get; set; } = new ScreenshotFile();

		public class ScreenshotFile
		{
			public string Path { get; set; }
			public string Description { get; set; }
		}
	}
}
