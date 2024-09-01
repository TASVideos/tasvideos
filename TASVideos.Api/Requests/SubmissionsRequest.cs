namespace TASVideos.Api.Requests;

internal class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	[SwaggerParameter("The statuses to filter by")]
	public string? Statuses { get; init; }

	[SwaggerParameter("The author/submitter name to filter by")]
	public string? User { get; init; }

	[SwaggerParameter("The start year to filter by")]
	public int? StartYear { get; init; }

	[SwaggerParameter("The end year to filter by")]
	public int? EndYear { get; init; }

	[SwaggerParameter("The system codes to filter by")]
	public string? Systems { get; init; }

	[SwaggerParameter("The ids of the games to filter by")]
	public string? Games { get; init; }

	[SwaggerParameter("The start type of the movie. 0 = Power On, 1 = Sram, 2 = Savestate")]
	public int? StartType { get; init; }

	public bool? ShowVerified { get; set; }

	ICollection<int> ISubmissionFilter.Years => StartYear.YearRange(EndYear).ToList();

	ICollection<SubmissionStatus> ISubmissionFilter.Statuses => !string.IsNullOrWhiteSpace(Statuses)
		? Statuses
			.SplitWithEmpty(",")
			.Where(s => Enum.TryParse(s, out SubmissionStatus _))
			.Select(s =>
			{
				Enum.TryParse(s, out SubmissionStatus x);
				return x;
			})
			.ToList()
		: [];

	ICollection<string> ISubmissionFilter.Systems => Systems.CsvToStrings();
	ICollection<int> ISubmissionFilter.GameIds => Games.CsvToInts();
}
