using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Tests.Services;

internal class TestParseResult : IParseResult
{
	public bool Success { get; init; }
	public IEnumerable<string> Errors { get; } = [];
	public IEnumerable<ParseWarnings> Warnings { get; } = [];
	public string FileExtension { get; init; } = "";
	public RegionType Region { get; init; }
	public int Frames { get; init; }
	public string SystemCode { get; init; } = "";
	public int RerecordCount { get; init; }
	public MovieStartType StartType { get; init; }
	public double? FrameRateOverride { get; init; }
	public long? CycleCount { get; init; }
	public string? Annotations { get; init; }
	public Dictionary<HashType, string> Hashes { get; init; } = [];
}
