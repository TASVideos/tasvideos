namespace TASVideos.Core.Services;

/// <summary>
/// Represents a game system including the framerates associated with this system
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
		public bool Obsolete { get; init; }
	}
}
