namespace TASVideos.Data.Entity.Game;

[ExcludeFromHistory]
public class GameGameGroup
{
	public int GameId { get; set; }
	public Game? Game { get; set; }

	public int GameGroupId { get; set; }
	public GameGroup? GameGroup { get; set; }
}
