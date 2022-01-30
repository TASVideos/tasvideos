using TASVideos.Data.Entity;

namespace TASVideos.Pages.Activity.Model;

public class SubmissionEntryModel
{
	public int Id { get; set; }
	public DateTime CreateTimestamp { get; set; }
	public string Title { get; set; } = "";
	public SubmissionStatus Status { get; set; }
}
