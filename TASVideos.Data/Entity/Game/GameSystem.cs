namespace TASVideos.Data.Entity.Game;

/// <summary>
/// Represents the system that a game runs on, such as NES, SNES, Commodore 64, PSX, etc
/// </summary>
public class GameSystem : BaseEntity
{
	public int Id { get; set; } // Note that this is Non-auto-incrementing, we need Ids to be identical across any database

	public virtual ICollection<GameSystemFrameRate> SystemFrameRates { get; set; } = new HashSet<GameSystemFrameRate>();

	public virtual ICollection<Game> Games { get; set; } = new HashSet<Game>();

	public virtual ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
	public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();

	[Required]
	[StringLength(8)]
	public string Code { get; set; } = "";

	[Required]
	[StringLength(100)]
	public string DisplayName { get; set; } = "";
}

public static class GameSystemExtensions
{
	public static IQueryable<GameSystem> ForCode(this IQueryable<GameSystem> query, string code)
	{
		return query.Where(s => s.Code == code);
	}
}
