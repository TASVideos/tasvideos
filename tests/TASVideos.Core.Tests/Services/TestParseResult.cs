using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Tests.Services;

internal class TestParseResult : IParseResult
{
	public bool Success { get; set; }
	public IEnumerable<string> Errors { get; set; } = new List<string>();
	public IEnumerable<ParseWarnings> Warnings { get; set; } = new List<ParseWarnings>();
	public string FileExtension { get; set; } = "";
	public RegionType Region { get; set; }
	public int Frames { get; set; }
	public string SystemCode { get; set; } = "";
	public int RerecordCount { get; set; }
	public MovieStartType StartType { get; set; }
	public double? FrameRateOverride { get; set; }
	public long? CycleCount { get; set; }
}
