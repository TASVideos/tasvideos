namespace TASVideos.Data.Entity;

public class UserMaintenanceLog
{
	public int Id { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Log { get; set; } = "";

	public int? EditorId { get; set; }
	public virtual User? Editor { get; set; }

	public int? UserId { get; set; }
	public virtual User? User { get; set; }
}
