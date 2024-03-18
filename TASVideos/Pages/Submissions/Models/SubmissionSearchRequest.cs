using System.ComponentModel.DataAnnotations;
using TASVideos.Core;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionSearchRequest : PagingModel, ISubmissionFilter
{
	public SubmissionSearchRequest()
	{
		Sort = $"{nameof(SubmissionListEntry.Submitted)}";
		PageSize = 100;
	}

	public IEnumerable<int> Years { get; set; } = [];

	public IEnumerable<int> AvailableYears => Enumerable
		.Range(2000, DateTime.UtcNow.Year + 1 - 2000)
		.OrderByDescending(n => n).ToList();

	public string? System { get; set; }

	public string? User { get; set; }

	public string? GameId { get; set; }

	public int? StartType { get; set; }

	[Display(Name = "Statuses")]
	public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = [];

	public static IEnumerable<SubmissionStatus> Default =>
		[
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		];

	public static IEnumerable<SubmissionStatus> All => Enum
		.GetValues(typeof(SubmissionStatus))
		.Cast<SubmissionStatus>()
		.ToList();

	IEnumerable<string> ISubmissionFilter.Systems => string.IsNullOrWhiteSpace(System)
		? []
		: [System];

	IEnumerable<int> ISubmissionFilter.GameIds => !string.IsNullOrWhiteSpace(GameId) && int.TryParse(GameId, out int _)
		? [int.Parse(GameId)]
		: [];
}
