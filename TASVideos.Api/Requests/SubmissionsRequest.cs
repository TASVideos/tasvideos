using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data.Entity;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the filtering criteria for the submissions endpoint
	/// </summary>
	public class SubmissionsRequest : ApiRequest, ISubmissionFilter
	{
		/// <summary>
		/// Gets or sets the statuses to filter by
		/// </summary>
		public string Statuses { get; set; }

		/// <summary>
		/// Gets or sets user's name to filter by
		/// </summary>
		public string User { get; set; }

		/// <summary>
		/// Gets or sets the start year to filter by
		/// </summary>
		public int? StartYear { get; set; }

		/// <summary>
		/// Gets or sets the end year to filter by
		/// </summary>
		public int? EndYear { get; set; }

		/// <summary>
		/// Gets or sets the system codes to filter by
		/// </summary>
		public string Systems { get; set; }

		IEnumerable<int> ISubmissionFilter.Years => StartYear.YearRange(EndYear);

		IEnumerable<SubmissionStatus> ISubmissionFilter.StatusFilter => !string.IsNullOrWhiteSpace(Statuses)
			? Statuses
				.Split(
					new[]
					{
						","
					}, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => Enum.TryParse(s, out SubmissionStatus x))
				.Select(s =>
					{
						Enum.TryParse(s, out SubmissionStatus x);
						return x;
					})
			: Enumerable.Empty<SubmissionStatus>();

		IEnumerable<string> ISubmissionFilter.Systems => Systems.CsvToStrings();
	}
}
