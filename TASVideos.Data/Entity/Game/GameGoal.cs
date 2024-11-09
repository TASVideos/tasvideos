namespace TASVideos.Data.Entity.Game;

public class GameGoal
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public Game? Game { get; set; }

	[StringLength(50)]
	public string DisplayName { get; set; } = "";

	public ICollection<Publication> Publications { get; set; } = [];
	public ICollection<Submission> Submissions { get; set; } = [];
}

public static class GameGoalExtensions
{
	public static IQueryable<GameGoal> ForGame(this IQueryable<GameGoal> query, int gameId)
		=> query.Where(gg => gg.GameId == gameId);
}
