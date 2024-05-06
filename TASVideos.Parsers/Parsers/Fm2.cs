namespace TASVideos.MovieParsers.Parsers;

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

		(var header, result.Frames) = await file.PipeBasedMovieHeaderAndFrameCount();

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
		public const string Fds = "fds";
		public const string StartsFromSavestate = "savestate";
	}
}
