namespace TASVideos.MovieParsers.Result;

/// <summary>
/// An implementation of <seealso cref="IParseResult"/> that represents a successful result.
/// </summary>
internal class SuccessResult(string fileExtension) : IParseResult
{
	public bool Success => true;
	public IEnumerable<string> Errors => [];
	public IEnumerable<ParseWarnings> Warnings => WarningList;

	public string FileExtension => fileExtension;
	public RegionType Region { get; internal set; }
	public int Frames { get; internal set; }
	public string SystemCode { get; internal set; } = "";
	public int RerecordCount { get; internal set; }
	public MovieStartType StartType { get; internal set; }
	public double? FrameRateOverride { get; internal set; }
	public long? CycleCount { get; internal set; }
	public string? Annotations { get; internal set; }

	internal List<ParseWarnings> WarningList { get; } = [];

	public Dictionary<HashType, string> Hashes { get; } = [];
}

internal static class ParseResultExtensions
{
	internal static void WarnNoRerecords(this SuccessResult successResult)
	{
		successResult.WarningList.Add(ParseWarnings.MissingRerecordCount);
	}

	internal static void WarnNoFrameRate(this SuccessResult successResult)
	{
		successResult.WarningList.Add(ParseWarnings.FrameRateInferred);
	}

	internal static void WarnLengthInferred(this SuccessResult successResult)
	{
		successResult.WarningList.Add(ParseWarnings.LengthInferred);
	}
}
