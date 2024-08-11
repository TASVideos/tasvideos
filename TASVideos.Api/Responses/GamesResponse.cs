using TASVideos.Data.Entity.Game;

namespace TASVideos.Api.Responses;

internal class GamesResponse
{
	[Sortable]
	public int Id { get; init; }

	public IEnumerable<GameVersion> Versions { get; init; } = [];

	[Sortable]
	public string DisplayName { get; init; } = "";

	[Sortable]
	public string? Abbreviation { get; init; } = "";

	[Sortable]
	public string? Aliases { get; init; }

	[Sortable]
	public string? ScreenshotUrl { get; init; } = "";

	public class GameVersion
	{
		public int Id { get; init; }
		public string? Md5 { get; init; } = "";
		public string? Sha1 { get; init; } = "";
		public string Name { get; init; } = "";
		public VersionTypes Type { get; init; }
		public string? Region { get; init; } = "";
		public string? Version { get; init; } = "";
		public string SystemCode { get; init; } = "";
		public string? SourceDb { get; init; }
	}
}
