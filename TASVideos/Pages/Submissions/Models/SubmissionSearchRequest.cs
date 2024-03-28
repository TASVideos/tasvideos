using TASVideos.Core;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionSearchRequest : PagingModel, ISubmissionFilter
{
	public SubmissionSearchRequest()
	{
		Sort = $"{nameof(SubmissionListEntry.Submitted)}";
		PageSize = 100;
	}

	public ICollection<int> Years { get; set; } = [];

	public List<int> AvailableYears => [.. Enumerable
		.Range(2000, DateTime.UtcNow.Year + 1 - 2000)
		.OrderByDescending(n => n)];

	public string? System { get; set; }
	public string? User { get; set; }
	public string? GameId { get; set; }
	public int? StartType { get; set; }

	[Display(Name = "Statuses")]
	public ICollection<SubmissionStatus> StatusFilter { get; set; } = [];

	public static ICollection<SubmissionStatus> Default =>
	[
		SubmissionStatus.New,
		SubmissionStatus.JudgingUnderWay,
		SubmissionStatus.Accepted,
		SubmissionStatus.PublicationUnderway,
		SubmissionStatus.NeedsMoreInfo,
		SubmissionStatus.Delayed
	];

	public static List<SubmissionStatus> All => Enum
		.GetValues(typeof(SubmissionStatus))
		.Cast<SubmissionStatus>()
		.ToList();

	ICollection<string> ISubmissionFilter.Systems => string.IsNullOrWhiteSpace(System)
		? []
		: [System];

	ICollection<int> ISubmissionFilter.GameIds => !string.IsNullOrWhiteSpace(GameId) && int.TryParse(GameId, out int _)
		? [int.Parse(GameId)]
		: [];
}
