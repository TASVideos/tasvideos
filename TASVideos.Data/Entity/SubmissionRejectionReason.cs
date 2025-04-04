namespace TASVideos.Data.Entity;

public class SubmissionRejectionReason
{
	public int Id { get; set; }

	public string DisplayName { get; set; } = "";

	public ICollection<Submission> Submissions { get; init; } = [];
}
