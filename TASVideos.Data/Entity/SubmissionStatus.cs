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
	Playground
}

public static class SubmissionStatusExtensions
{
	public static bool CanBeJudged(this SubmissionStatus status)
		=> status is SubmissionStatus.New
			or SubmissionStatus.Delayed
			or SubmissionStatus.NeedsMoreInfo
			or SubmissionStatus.JudgingUnderWay;

	public static bool IsWorkInProgress(this SubmissionStatus status)
		=> status is SubmissionStatus.New
			or SubmissionStatus.Delayed
			or SubmissionStatus.NeedsMoreInfo
			or SubmissionStatus.JudgingUnderWay
			or SubmissionStatus.Accepted
			or SubmissionStatus.PublicationUnderway;

	public static bool IsJudgeDecision(this SubmissionStatus status)
		=> status is SubmissionStatus.Accepted
			or SubmissionStatus.Rejected
			or SubmissionStatus.Cancelled
			or SubmissionStatus.Delayed
			or SubmissionStatus.NeedsMoreInfo
			or SubmissionStatus.Playground;
}
