namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class SubmissionRejectionReason
{
	public int Id { get; set; }

	[StringLength(100)]
	public string DisplayName { get; set; } = "";

	public ICollection<Submission> Submissions { get; set; } = [];
}
