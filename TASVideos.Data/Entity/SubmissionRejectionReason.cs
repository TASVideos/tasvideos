namespace TASVideos.Data.Entity;

public class SubmissionRejectionReason
{
	public int Id { get; set; }

	[Required]
	[StringLength(100)]
	public string DisplayName { get; set; } = "";

	public ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
}
