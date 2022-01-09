using System;
using System.Collections.Generic;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for the publications endpoint.
	/// </summary>
	public class PublicationsRequest : ApiRequest, IPublicationTokens
	{
		/// <summary>
		/// Gets the system codes to filter by.
		/// </summary>
		public string? Systems { get; init; }

		/// <summary>
		/// Gets the publication class codes to filter by.
		/// </summary>
		public string? ClassNames { get; init; }

		/// <summary>
		/// Gets the start year to filter by.
		/// </summary>
		public int? StartYear { get; init; }

		/// <summary>
		/// Gets the end year to filter by.
		/// </summary>
		public int? EndYear { get; init; }

		/// <summary>
		/// Gets the game genres to filter by.
		/// </summary>
		public string? GenreNames { get; init; }

		/// <summary>
		/// Gets the publication flags names to filter by.
		/// </summary>
		public string? TagNames { get; init; }

		/// <summary>
		/// Gets the publication flags names to filter by.
		/// </summary>
		public string? FlagNames { get; init; }

		/// <summary>
		/// Gets the author ids filter by.
		/// </summary>
		public string? AuthorIds { get; init; }

		/// <summary>
		/// Gets a value indicating whether or not to show obsoleted in addition to current movies.
		/// </summary>
		public bool ShowObsoleted { get; init; }

		/// <summary>
		/// Gets a value indicating whether or not to only show obsoleted movies (and exclude current)
		/// </summary>
		public bool OnlyObsoleted { get; init; }

		/// <summary>
		/// Gets the list of Game Ids to filter by.
		/// </summary>
		public string? GameIds { get; init; }

		/// <summary>
		///  Gets the list of Game Group Ids to filter by.
		/// </summary>
		public string? GameGroupIds { get; init; }

		IEnumerable<string> IPublicationTokens.SystemCodes => Systems.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Classes => ClassNames.CsvToStrings();
		IEnumerable<int> IPublicationTokens.Years => StartYear.YearRange(EndYear);
		IEnumerable<string> IPublicationTokens.Genres => GenreNames.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Tags => TagNames.CsvToStrings();
		IEnumerable<string> IPublicationTokens.Flags => FlagNames.CsvToStrings();
		IEnumerable<int> IPublicationTokens.Authors => AuthorIds.CsvToInts();
		IEnumerable<int> IPublicationTokens.MovieIds => Array.Empty<int>();
		IEnumerable<int> IPublicationTokens.Games => GameIds.CsvToInts();
		IEnumerable<int> IPublicationTokens.GameGroups => GameGroupIds.CsvToInts();
		string IPublicationTokens.SortBy => "";
		int? IPublicationTokens.Limit => null;
	}
}
