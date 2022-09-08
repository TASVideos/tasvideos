using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class RoleLink
{
	public int Id { get; set; }

	[Required]
	[StringLength(300)]
	public string Link { get; set; } = "";

	public int RoleId { get; set; }
	public virtual Role? Role { get; set; }
}
