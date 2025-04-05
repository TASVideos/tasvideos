namespace TASVideos.Data.AutoHistory;

public class AutoHistoryEntry
{
	public int Id { get; set; }
	public string RowId { get; set; } = "";
	public string TableName { get; set; } = "";
	public string? Changed { get; set; }
	public EntityState Kind { get; set; }
	public DateTime Created { get; set; } = DateTime.UtcNow;
	public int UserId { get; set; }
}
