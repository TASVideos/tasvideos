namespace TASVideos.Data.Entity;

public enum SubmissionStatus
{
	[Display(Name = "New")]
	New,

	[Display(Name = "Delayed")]
	Delayed,

	[Display(Name = "Needs More Info")]
	NeedsMoreInfo,

	[Display(Name = "Judging Underway")]
	JudgingUnderWay,

	[Display(Name = "Accepted")]
	Accepted,

	[Display(Name = "Publication Underway")]
	PublicationUnderway,

	[Display(Name = "Published")]
	Published,

	[Display(Name = "Rejected")]
	Rejected,

	[Display(Name = "Cancelled")]
	Cancelled,

	[Display(Name = "Playground")]
	Playground,
}

public static class SubmissionStatusExtensions
{
	public static bool CanBeJudged(this SubmissionStatus status)
	{
		return status == SubmissionStatus.New
			|| status == SubmissionStatus.Delayed
			|| status == SubmissionStatus.NeedsMoreInfo
			|| status == SubmissionStatus.JudgingUnderWay;
	}
}
