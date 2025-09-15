using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

[IncludeInAutoHistory]
public class Genre
{
	public int Id { get; set; }

	public string DisplayName { get; set; } = "";

	public ICollection<GameGenre> GameGenres { get; init; } = [];
}
