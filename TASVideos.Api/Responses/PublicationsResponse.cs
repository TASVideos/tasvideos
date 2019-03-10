using TASVideos.Data;

#pragma warning disable 1591
namespace TASVideos.Api.Responses
{
	/// <summary>
	/// Represents a publication returned by the publications endpoint
	/// </summary>
	public class PublicationsResponse
	{
		[Sortable]
		public int Id { get; set; }

		[Sortable]
		public string Title { get; set; }

		[Sortable]
		public string Branch { get; set; }

		[Sortable]
		public string EmulatorVersion { get; set; }
	}
}
