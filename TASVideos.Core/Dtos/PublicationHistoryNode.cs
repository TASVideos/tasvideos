namespace TASVideos.Core.Services;

public class PublicationHistoryGroup
{
	public int GameId { get; init; }
	public string GameDisplayName { get; init; } = "";

	public IEnumerable<PublicationHistoryNode> Goals { get; init; } = [];
}

public class PublicationHistoryNode
{
	public int Id { get; init; }
	public string Title { get; init; } = "";
	public string? Goal { get; init; }
	public DateTime CreateTimestamp { get; set; }

	public string? ClassIconPath { get; set; }

	public IEnumerable<FlagEntry> Flags { get; set; } = [];

	public IEnumerable<PublicationHistoryNode> Obsoletes => ObsoleteList;

	internal int? ObsoletedById { get; init; }

	internal List<PublicationHistoryNode> ObsoleteList { get; set; } = [];

	public record FlagEntry(string? IconPath, string? LinkPath, string Name);
}
