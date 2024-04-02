using TASVideos.Common;
using TASVideos.Core;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionListEntry : ITimeable, ISubmissionDisplay
{
	[Sortable]
	public string? System { get; init; }

	[Sortable]
	[Display(Name = "Game")]
	public string? GameName { get; init; }

	[Sortable]
	public string? Branch { get; init; }

	[Display(Name = "Time")]
	public TimeSpan Time => this.Time();

	[Display(Name = "By")]
	public List<string>? Authors { get; init; }
	[TableIgnore]
	public string? AdditionalAuthors { get; init; }

	[Sortable]
	[Display(Name = "Date")]
	public DateTime Submitted { get; init; }

	[Sortable]
	[Display(Name = "Status")]
	public SubmissionStatus Status { get; init; }

	[TableIgnore]
	public int Id { get; init; }

	[TableIgnore]
	public int Frames { get; init; }

	[TableIgnore]
	public double FrameRate { get; init; }

	[TableIgnore]
	public string? Judge { get; init; }

	[TableIgnore]
	public string? Publisher { get; init; }

	[TableIgnore]
	public string? IntendedClass { get; init; }
}

public class SubmissionPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
{
	public IEnumerable<int> Years { get; set; } = [];
	public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = [];
	public string? System { get; set; }
	public string? User { get; set; }
	public string? GameId { get; set; }

	public static new SubmissionPageOf<T> Empty() => new([]);
}
