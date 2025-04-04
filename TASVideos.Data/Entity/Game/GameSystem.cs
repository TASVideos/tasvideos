using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

/// <summary>
/// Represents the system that a game runs on, such as NES, SNES, Commodore 64, PSX, etc
/// </summary>
[IncludeInAutoHistory]
public class GameSystem : BaseEntity
{
	public int Id { get; set; } // Note that this is Non-auto-incrementing, we need Ids to be identical across any database

	public ICollection<GameSystemFrameRate> SystemFrameRates { get; init; } = [];

	public ICollection<GameVersion> GameVersions { get; init; } = [];

	public ICollection<Publication> Publications { get; init; } = [];
	public ICollection<Submission> Submissions { get; init; } = [];

	public string Code { get; set; } = "";

	public string DisplayName { get; set; } = "";
}

public static class GameSystemExtensions
{
	public static IQueryable<GameSystem> ForCode(this IQueryable<GameSystem> query, string code)
		=> query.Where(s => s.Code == code);
}
