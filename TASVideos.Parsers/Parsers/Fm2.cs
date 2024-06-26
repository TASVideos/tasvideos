﻿namespace TASVideos.MovieParsers.Parsers;

[FileExtension("fm2")]
internal class Fm2 : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Nes
		};

		(var header, int initialFrameCount) = await file.PipeBasedMovieHeaderAndFrameCount();

		if (header.GetBoolFor(Keys.Binary))
		{
			int? frameCount = header.GetPositiveIntFor(Keys.Length);
			if (frameCount.HasValue)
			{
				result.Frames = frameCount.Value;
			}
			else
			{
				return Error("No frame count found for binary format");
			}
		}
		else
		{
			result.Frames = initialFrameCount;
		}

		if (header.GetBoolFor(Keys.Fds))
		{
			result.SystemCode = SystemCodes.Fds;
		}

		int? rerecordVal = header.GetPositiveIntFor(Keys.RerecordCount);
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		if (header.GetBoolFor(Keys.Pal))
		{
			result.Region = RegionType.Pal;
		}

		if (header.HasValue(Keys.StartsFromSavestate))
		{
			result.StartType = MovieStartType.Savestate;
		}

		return result;
	}

	private static class Keys
	{
		public const string RerecordCount = "rerecordcount";
		public const string Pal = "palFlag";
		public const string Binary = "binary";
		public const string Length = "length";
		public const string Fds = "fds";
		public const string StartsFromSavestate = "savestate";
	}
}
