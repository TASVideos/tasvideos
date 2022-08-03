using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class SubmissionRejectionReason
{
	public int Id { get; set; }

	[Required]
	[StringLength(100)]
	public string DisplayName { get; set; } = "";

	public ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
}
