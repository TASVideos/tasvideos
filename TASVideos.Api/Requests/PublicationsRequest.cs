namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for the publications endpoint
	/// </summary>
	public class PublicationsRequest : ApiRequest
	{
		/// <summary>
		/// Gets or sets the system code filter
		/// </summary>
		public string SystemCode { get; set; }
	}
}
