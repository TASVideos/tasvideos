using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

[IncludeInAutoHistory]
public class GameGroup
{
	public int Id { get; set; }

	public string Name { get; set; } = "";

	public string? Abbreviation { get; set; }

	public string? Description { get; set; }

	public ICollection<GameGameGroup> Games { get; init; } = [];
}
