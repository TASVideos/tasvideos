namespace TASVideos.Data.Entity;

public class RoleLink
{
	public int Id { get; set; }

	public string Link { get; set; } = "";

	public int RoleId { get; set; }
	public Role? Role { get; set; }
}
