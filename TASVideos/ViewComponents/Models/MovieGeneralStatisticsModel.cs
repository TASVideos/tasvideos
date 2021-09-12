using System;
using System.Collections.Generic;
using TASVideos.Common;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class MovieGeneralStatisticsModel
	{
		public int PublishedMovieCount { get; init; }
		public int TotalMovieCount { get; init; }
		public int SubmissionCount { get; init; }
		public int AverageRerecordCount { get; init; }
	}
}
