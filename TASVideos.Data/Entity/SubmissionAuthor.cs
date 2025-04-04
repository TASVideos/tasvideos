namespace TASVideos.Data.Entity;

public class SubmissionAuthor
{
	public int UserId { get; set; }
	public User? Author { get; set; }

	public int Ordinal { get; set; }

	public int SubmissionId { get; set; }
	public Submission? Submission { get; set; }
}
