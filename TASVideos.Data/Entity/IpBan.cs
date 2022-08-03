using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class IpBan : BaseEntity
{
	public int Id { get; set; }

	[Required]
	[StringLength(40)]
	public string Mask { get; set; } = "";
}
