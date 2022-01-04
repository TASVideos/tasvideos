using System;
using System.Collections.Generic;
using TASVideos.Common;

namespace TASVideos.ViewComponents
{
	public class TabularMovieListSearchModel
	{
		public int Limit { get; set; } = 10;
		public IEnumerable<string> PublicationClasses { get; set; } = new List<string>();
	}

	public class TabularMovieListResultModel : ITimeable
	{
		public int Id { get; set; }
		public DateTime CreateTimestamp { get; set; }

		public TimeSpan? PreviousTime { get; set; }
		public int? PreviousId { get; set; }

		public string System { get; set; } = "";
		public string Game { get; set; } = "";
		public string? Branch { get; set; }
		public IEnumerable<string>? Authors { get; set; }
		public string? AdditionalAuthors { get; set; }

		public ScreenshotFile Screenshot { get; set; } = new ();

		public class ScreenshotFile
		{
			public string Path { get; set; } = "";
			public string? Description { get; set; }
		}

		public int Frames { get; set; }
		public double FrameRate { get; set; }

		public ObsoletedPublication? ObsoletedMovie { get; set; }

		public class ObsoletedPublication : ITimeable
		{
			public int Id { get; set; }
			public int Frames { get; set; }
			public double FrameRate { get; set; }
		}
	}
}
