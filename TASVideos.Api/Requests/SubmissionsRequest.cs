namespace TASVideos.Api.Requests;

internal class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	[SwaggerParameter("The statuses to filter by")]
	public string? Statuses { get; set; }

	[SwaggerParameter("The author/submitter name to filter by")]
	public string? User { get; set; }

	[SwaggerParameter("The start year to filter by")]
	public int? StartYear { get; set; }

	[SwaggerParameter("The end year to filter by")]
	public int? EndYear { get; set; }

	[SwaggerParameter("The system codes to filter by")]
	public string? Systems { get; set; }

	[SwaggerParameter("The ids of the games to filter by")]
	public string? Games { get; set; }

	[SwaggerParameter("Gets the start type of the movie. 0 = Power On, 1 = Sram, 2 = Savestate")]
	public int? StartType { get; set; }

	ICollection<int> ISubmissionFilter.Years => StartYear.YearRange(EndYear).ToList();

	ICollection<SubmissionStatus> ISubmissionFilter.StatusFilter => !string.IsNullOrWhiteSpace(Statuses)
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
