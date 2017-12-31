using System;
using System.Collections.Generic;
using System.Text;

namespace TASVideos.MovieParsers
{
	// TODO: document, entry point for parsers
	public class MovieParser
	{
		// TODO: document
		public IParseResult Parse(byte[] zip) // TODO: stream?
		{
			// For testing
			return new ParseResult
			{
				Region = RegionType.Ntsc,
				Frames = new Random().Next(10000, 250000),
				SystemCode = "NES",
				RerecordCount = new Random().Next(10000, 50000)
			};
		}
	}

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
		public bool Success { get; internal set; } = true;
		public IEnumerable<string> Errors { get; internal set; } = new List<string>();
		public IEnumerable<string> Warnings { get; internal set; } = new List<string>();

		public RegionType Region { get; internal set; }
		public int Frames { get; internal set; }
		public string SystemCode { get; internal set; }
		public int RerecordCount { get; internal set; }
	}

	internal interface IParseable
	{
		string FileExtension { get; }
		IParseResult Parse(byte[] file); // TODO: stream?
	}

	internal class Bk2 : IParseable
	{
		public string FileExtension => "bk2";

		public IParseResult Parse(byte[] file)
		{
			return new ParseResult();
		}
	}
}
