namespace TASVideos.Api.Requests;

internal class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	[Description("The statuses to filter by")]
	public string? Statuses { get; set; }

	[Description("The author/submitter name to filter by")]
	public string? User { get; set; }

	[Description("The start year to filter by")]
	public int? StartYear { get; set; }

	[Description("The end year to filter by")]
	public int? EndYear { get; set; }

	[Description("The system codes to filter by")]
	public string? Systems { get; set; }

	[Description("The ids of the games to filter by")]
	public string? Games { get; set; }

	[Description("Gets the start type of the movie. 0 = Power On, 1 = Sram, 2 = Savestate")]
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
