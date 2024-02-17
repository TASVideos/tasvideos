namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserMaintenanceLog
{
	public int Id { get; set; }
	public DateTime TimeStamp { get; set; }

	[StringLength(50)]
	public string Log { get; set; } = "";

	public int? EditorId { get; set; }
	public virtual User? Editor { get; set; }

	public int? UserId { get; set; }
	public virtual User? User { get; set; }
}
