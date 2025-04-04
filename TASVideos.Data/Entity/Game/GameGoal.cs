using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

[IncludeInAutoHistory]
public class GameGoal
{
	public int Id { get; set; }
	public int GameId { get; set; }
	public Game? Game { get; set; }

	public string DisplayName { get; set; } = "";

	public ICollection<Publication> Publications { get; init; } = [];
	public ICollection<Submission> Submissions { get; init; } = [];
}

public static class GameGoalExtensions
{
	public static IQueryable<GameGoal> ForGame(this IQueryable<GameGoal> query, int gameId)
		=> query.Where(gg => gg.GameId == gameId);
}
