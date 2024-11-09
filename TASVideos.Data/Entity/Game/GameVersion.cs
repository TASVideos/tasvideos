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

public class GameVersion : BaseEntity
{
	public int Id { get; set; }

	public int GameId { get; set; }
	public Game? Game { get; set; }
	public int SystemId { get; set; }
	public GameSystem? System { get; set; }

	public ICollection<Publication> Publications { get; set; } = [];
	public ICollection<Submission> Submissions { get; set; } = [];

	[StringLength(32)]
	public string? Md5 { get; set; }

	[StringLength(40)]
	public string? Sha1 { get; set; }

	[StringLength(255)]
	public string Name { get; set; } = "";

	public VersionTypes Type { get; set; }

	[StringLength(50)]
	public string? Region { get; set; }

	[StringLength(50)]
	public string? Version { get; set; }

	[StringLength(255)]
	public string? TitleOverride { get; set; }

	[StringLength(50)]
	public string? SourceDb { get; set; }

	[StringLength(2000)]
	public string? Notes { get; set; }
}

public static class GameVersionExtensions
{
	public static IQueryable<GameVersion> ForGame(this IQueryable<GameVersion> query, int gameId)
		=> query.Where(g => g.GameId == gameId);

	public static IQueryable<GameVersion> ForSystem(this IQueryable<GameVersion> query, int systemId)
		=> query.Where(g => g.SystemId == systemId);
}
