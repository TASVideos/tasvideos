using System.Collections.Generic;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for the publications endpoint
	/// </summary>
	public class PublicationsRequest : ApiRequest, IPublicationTokens
	{
		/// <summary>
		/// Gets or sets the system codes to filter by
		/// </summary>
		public string Systems { get; set; }

		/// <summary>
		/// Gets or sets the publication tier codes to filter by
		/// </summary>
		public string TierNames { get; set; }

		/// <summary>
		/// Gets or sets the start year to filter by
		/// </summary>
		public int? StartYear { get; set; }

		/// <summary>
		/// Gets or sets the end year to filter by
		/// </summary>
		public int? EndYear { get; set; }

		/// <summary>
		/// Gets or sets the game genres to filter by
		/// </summary>
		public string GenreNames { get; set; }

		/// <summary>
		/// Gets or sets the publication flags names to filter by
		/// </summary>
		public string TagNames { get; set; }

		/// <summary>
		/// Gets or sets the publication flags names to filter by
		/// </summary>
		public string FlagNames { get; set; }

		/// <summary>
		/// Gets or sets the author ids filter by
		/// </summary>
		public string AuthorIds { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether or not to show obsoleted movies
		/// </summary>
		public bool ShowObsoleted { get; set; }

		IEnumerable<string> IPublicationTokens.SystemCodes => Systems.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Tiers => TierNames.CsvToStrings();
		IEnumerable<int> IPublicationTokens.Years => StartYear.YearRange(EndYear);
		IEnumerable<string> IPublicationTokens.Genres => GenreNames.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Tags => TagNames.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Flags => FlagNames.CsvToStrings();
		IEnumerable<int> IPublicationTokens.Authors => AuthorIds.CsvToInts();
		IEnumerable<int> IPublicationTokens.MovieIds => new int[0];
	}
}
