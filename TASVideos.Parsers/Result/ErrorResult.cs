namespace TASVideos.MovieParsers.Result;

/// <summary>
/// An implementation of <seealso cref="IParseResult"/> that can be used when an error occurs.
/// </summary>
internal class ErrorResult(string errorMsg) : IParseResult
{
	public bool Success => false;
	public IEnumerable<string> Errors { get; } = [errorMsg];

	public IEnumerable<ParseWarnings> Warnings => [];
	public string FileExtension { get; internal init; } = "";
	public RegionType Region => RegionType.Unknown;
	public int Frames => 0;
	public string SystemCode => "";
	public int RerecordCount => -1;
	public MovieStartType StartType => MovieStartType.PowerOn;
	public double? FrameRateOverride => null;
	public long? CycleCount => null;
	public string? Annotations => null;
	public Dictionary<HashType, string> Hashes => [];
}
