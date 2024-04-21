namespace TASVideos.Api.Requests;

public class SubmissionsRequest : ApiRequest, ISubmissionFilter
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

	public static new ValueTask<SubmissionsRequest> BindAsync(HttpContext context)
	{
		return ValueTask.FromResult(new SubmissionsRequest
		{
			Sort = context.Request.Query["Sort"],
			Fields = context.Request.Query["Fields"],
			PageSize = context.Request.GetInt(nameof(PageSize)) ?? 100,
			CurrentPage = context.Request.GetInt(nameof(CurrentPage)) ?? 1,
			StartYear = context.Request.GetInt(nameof(StartYear)),
			EndYear = context.Request.GetInt(nameof(EndYear)),
			StartType = context.Request.GetInt(nameof(StartType)),
			Statuses = context.Request.Query["Statuses"],
			User = context.Request.Query["User"],
			Systems = context.Request.Query["Systems"],
			Games = context.Request.Query["Games"]
		});
	}
}
