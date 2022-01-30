namespace TASVideos.Data.Entity;

public class RoleLink
{
	public int Id { get; set; }

	[Required]
	[StringLength(300)]
	public string Link { get; set; } = "";

	public virtual Role? Role { get; set; }
}
