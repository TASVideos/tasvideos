namespace TASVideos.MovieParsers.Result;

/// <summary>
/// The standard implementation of <seealso cref="IParseResult"/>.
/// </summary>
internal class ParseResult : IParseResult
{
	public bool Success { get; internal set; } = true;
	public IEnumerable<string> Errors => ErrorList;
	public IEnumerable<ParseWarnings> Warnings => WarningList;

	public string FileExtension { get; internal set; } = "";
	public RegionType Region { get; internal set; }
	public int Frames { get; internal set; }
	public string SystemCode { get; internal set; } = "";
	public int RerecordCount { get; internal set; }
	public MovieStartType StartType { get; internal set; }
	public double? FrameRateOverride { get; internal set; }
	public long? CycleCount { get; internal set; }

	internal List<ParseWarnings> WarningList { get; set; } = new();
	internal List<string> ErrorList { get; set; } = new();
}

internal static class ParseResultExtensions
{
	internal static void WarnNoRerecords(this ParseResult parseResult)
	{
		parseResult.WarningList.Add(ParseWarnings.MissingRerecordCount);
	}

	internal static void WarnNoFrameRate(this ParseResult parseResult)
	{
		parseResult.WarningList.Add(ParseWarnings.FrameRateInferred);
	}

	internal static void WarnLengthInferred(this ParseResult parseResult)
	{
		parseResult.WarningList.Add(ParseWarnings.LengthInferred);
	}
}
