namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class RoleLink
{
	public int Id { get; set; }

	[StringLength(300)]
	public string Link { get; set; } = "";

	public int RoleId { get; set; }
	public Role? Role { get; set; }
}
