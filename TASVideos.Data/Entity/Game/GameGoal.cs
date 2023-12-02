namespace TASVideos.Data.Entity.Game;

public class GameGoal
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public virtual Game? Game { get; set; }

	public int GoalId { get; set; }
	public virtual Goal? Goal { get; set; }
}
