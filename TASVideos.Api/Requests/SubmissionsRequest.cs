namespace TASVideos.Api.Requests;

internal class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	public string? Statuses { get; set; }
	public string? User { get; set; }
	public int? StartYear { get; set; }
	public int? EndYear { get; set; }
	public string? Systems { get; set; }
	public string? Games { get; set; }
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
