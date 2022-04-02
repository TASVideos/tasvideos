using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("fm2")]
internal class Fm2 : IParser
{
	private const string FileExtension = "fm2";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		using var reader = new StreamReader(file);
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Nes
		};

		var lines = (await reader.ReadToEndAsync()).LineSplit();
		var header = lines
			.WithoutPipes()
			.ToArray();

		if (header.GetValueFor(Keys.Fds).ToBool())
		{
			result.SystemCode = SystemCodes.Fds;
		}

		int? rerecordVal = header.GetValueFor(Keys.RerecordCount).ToPositiveInt();
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		if (header.GetValueFor(Keys.Pal).ToBool())
		{
			result.Region = RegionType.Pal;
		}

		if (header.GetValueFor(Keys.StartsFromSavestate).Length > 1)
		{
			result.StartType = MovieStartType.Savestate;
		}

		result.Frames = lines.PipeCount();

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
