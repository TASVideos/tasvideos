using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity.Game;

[ExcludeFromHistory]
public class GameGroup
{
	public int Id { get; set; }

	[Required]
	[StringLength(255)]
	public string Name { get; set; } = "";

	[Required]
	[StringLength(255)]
	public string SearchKey { get; set; } = "";

	[StringLength(2000)]
	public string? Description { get; set; }

	public ICollection<GameGameGroup> Games { get; set; } = new HashSet<GameGameGroup>();
}
