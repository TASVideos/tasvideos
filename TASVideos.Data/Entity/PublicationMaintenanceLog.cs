namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class PublicationMaintenanceLog
{
	public int Id { get; set; }

	public DateTime TimeStamp { get; set; }

	[StringLength(1024)]

	public string Log { get; set; } = "";

	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	public int UserId { get; set; }
	public User? User { get; set; }
}
