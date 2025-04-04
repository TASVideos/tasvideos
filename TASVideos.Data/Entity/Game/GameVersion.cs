using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity.Game;

public enum VersionTypes
{
	Unknown,
	Good,
	Hack,
	Bad,
	Homebrew,
	Unlicensed,
	Prerelease,
	UnofficialPort,
	Unreleased,
	CustomLevelSet
}

[IncludeInAutoHistory]
public class GameVersion : BaseEntity
{
	public int Id { get; set; }

	public int GameId { get; set; }
	public Game? Game { get; set; }
	public int SystemId { get; set; }
	public GameSystem? System { get; set; }

	public ICollection<Publication> Publications { get; init; } = [];
	public ICollection<Submission> Submissions { get; init; } = [];

	public string? Md5 { get; set; }

	public string? Sha1 { get; set; }

	public string Name { get; set; } = "";

	public VersionTypes Type { get; set; }

	public string? Region { get; set; }

	public string? Version { get; set; }

	public string? TitleOverride { get; set; }

	public string? SourceDb { get; set; }

	public string? Notes { get; set; }
}

public static class GameVersionExtensions
{
	public static IQueryable<GameVersion> ForGame(this IQueryable<GameVersion> query, int gameId)
		=> query.Where(g => g.GameId == gameId);

	public static IQueryable<GameVersion> ForSystem(this IQueryable<GameVersion> query, int systemId)
		=> query.Where(g => g.SystemId == systemId);
}
