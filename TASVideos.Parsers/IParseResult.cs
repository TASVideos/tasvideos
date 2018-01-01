using System.Collections.Generic;

namespace TASVideos.MovieParsers
{
	// TODO: document
	public interface IParseResult
	{
		bool Success { get; }
		IEnumerable<string> Errors { get; } // If success is false, errors should exist
		IEnumerable<string> Warnings { get; } // If success is true, there might be warnings, need to be checked

		// start type? power-on, sram, savestate
		RegionType Region { get; }

		int Frames { get; }
		string SystemCode { get; } // NES, SNES, Genesis, etc
		int RerecordCount { get; }
	}

	public enum RegionType { Unknown, Ntsc, Pal }

	public class ParseResult : IParseResult
	{
		internal List<string> WarningList { get; set; } = new List<string>();
		internal List<string> ErrorList { get; set; } = new List<string>();

		public bool Success { get; internal set; } = true;
		public IEnumerable<string> Errors => ErrorList;
		public IEnumerable<string> Warnings => WarningList;

		public RegionType Region { get; internal set; }
		public int Frames { get; internal set; }
		public string SystemCode { get; internal set; }
		public int RerecordCount { get; internal set; }
	}

	public class ErrorResult : IParseResult
	{
		public ErrorResult(string errorMsg)
		{
			Errors = new[] { errorMsg };
		}

		public bool Success => false;
		public IEnumerable<string> Errors { get; internal set; }

		public IEnumerable<string> Warnings => new List<string>();
		public RegionType Region => RegionType.Unknown;
		public int Frames => 0;
		public string SystemCode => "";
		public int RerecordCount => -1;
	}
}
