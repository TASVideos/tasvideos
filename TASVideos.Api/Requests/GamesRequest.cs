using System.Collections.Generic;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for the games endpoint
	/// </summary>
	public class GamesRequest : ApiRequest
	{
		/// <summary>
		/// Gets or sets the system codes to filter by
		/// </summary>
		public string Systems { get; set; }

		internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();
	}
}
