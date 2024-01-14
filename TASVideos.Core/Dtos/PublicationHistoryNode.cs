﻿namespace TASVideos.Core.Services;

public class PublicationHistoryGroup
{
	public int GameId { get; init; }

	public IEnumerable<PublicationHistoryNode> Goals { get; init; } = new List<PublicationHistoryNode>();
}

public class PublicationHistoryNode
{
	public int Id { get; init; }
	public string Title { get; init; } = "";
	public string? Goal { get; init; }
	public DateTime CreateTimestamp { get; set; }

	public string? ClassIconPath { get; set; }

	public IEnumerable<FlagEntry> Flags { get; set; } = new List<FlagEntry>();

	public IEnumerable<PublicationHistoryNode> Obsoletes => ObsoleteList;

	internal int? ObsoletedById { get; init; }

	internal List<PublicationHistoryNode> ObsoleteList { get; set; } = new();

	public record FlagEntry(string? IconPath, string? LinkPath, string Name);
}
