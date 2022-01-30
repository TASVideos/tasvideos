#pragma warning disable 1591
namespace TASVideos.Api.Responses;

/// <summary>
/// Represents game systems returns by the systems endpoint.
/// </summary>
public class SystemsResponse
{
	public int Id { get; init; }
	public string Code { get; init; } = "";
	public string DisplayName { get; init; } = "";

	public IEnumerable<FrameRates> SystemFrameRates { get; init; } = new List<FrameRates>();

	public class FrameRates
	{
		public double FrameRate { get; init; }
		public string RegionCode { get; init; } = "";
		public bool Preliminary { get; init; }
	}
}
