using System.Collections.Generic;

#pragma warning disable 1591
namespace TASVideos.Api.Responses
{
	/// <summary>
	/// Represents game systems returns by the systems endpoint
	/// </summary>
	public class SystemsResponse
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public string DisplayName { get; set; }

		public IEnumerable<FrameRates> SystemFrameRates { get; set; } = new List<FrameRates>();

		public class FrameRates
		{
			public double FrameRate { get; set; }
			public string RegionCode { get; set; }
			public bool Preliminary { get; set; }
		}
	}
}
