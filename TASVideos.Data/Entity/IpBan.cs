namespace TASVideos.Data.Entity;

public class IpBan : BaseEntity
{
	public int Id { get; set; }

	public string Mask { get; set; } = "";
}
