using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace TASVideos.MovieParsers
{
	// TODO: document, entry point for parsers
	public sealed class MovieParser
	{
		// TODO: document
		public IParseResult Parse(Stream stream)
		{
			var zip = new ZipArchive(stream);
			if (zip.Entries.Count > 1)
			{
				return Error("Multiple files detected in the .zip, only one file is allowed");
			}
			
			// For testing
			return new ParseResult
			{
				Region = RegionType.Ntsc,
				Frames = new Random().Next(10000, 250000),
				SystemCode = "NES",
				RerecordCount = new Random().Next(10000, 50000)
			};
		}

		private IParseResult Error(string errorMsg)
		{
			return new ErrorResult(errorMsg);
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

	public class ErrorResult : IParseResult
	{
		public ErrorResult(string errorMsg)
		{
			Errors = new[] { errorMsg };
		}

		public bool Success => false;
		public IEnumerable<string> Errors { get; internal set; } = new List<string>();

		public IEnumerable<string> Warnings => new List<string>();
		public RegionType Region => RegionType.Unknown;
		public int Frames => 0;
		public string SystemCode => "";
		public int RerecordCount => -1;
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
