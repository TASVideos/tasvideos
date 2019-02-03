using System.Collections.Generic;

namespace TASVideos.MovieParsers.Result
{
	/// <summary>
	/// An implementation of <seealso cref="IParseResult"/> that can be used
	/// when an error occurs
	/// </summary>
	internal class ErrorResult : IParseResult
	{
		public ErrorResult(string errorMsg)
		{
			Errors = new[] { errorMsg };
		}

		public bool Success => false;
		public IEnumerable<string> Errors { get; internal set; }

		public IEnumerable<ParseWarnings> Warnings => new List<ParseWarnings>();
		public string FileExtension { get; internal set; }
		public RegionType Region => RegionType.Unknown;
		public int Frames => 0;
		public string SystemCode => "";
		public int RerecordCount => -1;
		public MovieStartType StartType => MovieStartType.PowerOn;
	}
}
