namespace TASVideos.MovieParsers.Parsers;

[FileExtension("dsm")]
internal class Dsm : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Ds
		};

		(var header, result.Frames) = await file.PipeBasedMovieHeaderAndFrameCount();

		int? rerecordVal = header.GetPositiveIntFor(Keys.RerecordCount);
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		if (header.HasValue(Keys.StartsFromSavestate))
		{
			result.StartType = MovieStartType.Savestate;
		}

		if (header.HasValue(Keys.StartsFromSram))
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
