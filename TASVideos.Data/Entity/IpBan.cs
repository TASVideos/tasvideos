namespace TASVideos.Data.Entity;

public class IpBan : BaseEntity
{
	public int Id { get; set; }

	[StringLength(40)]
	public string Mask { get; set; } = "";
}
