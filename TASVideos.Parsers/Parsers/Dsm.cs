﻿using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("dsm")]
internal class Dsm : IParser
{
	private const string FileExtension = "dsm";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Ds
		};

		(var header, result.Frames) = await file.PipeBasedMovieHeaderAndFrameCount();

		int? rerecordVal = header.GetValueFor(Keys.RerecordCount).ToPositiveInt();
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		if (header.GetValueFor(Keys.StartsFromSavestate).Length > 1)
		{
			result.StartType = MovieStartType.Savestate;
		}

		if (header.GetValueFor(Keys.StartsFromSram).Length > 1)
		{
			result.StartType = MovieStartType.Sram;
		}

		return result;
	}

	private static class Keys
	{
		public const string RerecordCount = "rerecordcount";
		public const string StartsFromSavestate = "savestate";
		public const string StartsFromSram = "sram";
	}
}
