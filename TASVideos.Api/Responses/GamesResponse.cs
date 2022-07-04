﻿using TASVideos.Core;
using TASVideos.Data.Entity.Game;

#pragma warning disable 1591
namespace TASVideos.Api.Responses;

/// <summary>
/// Represents a game returned by the games endpoint.
/// </summary>
public class GamesResponse
{
	[Sortable]
	public int Id { get; init; }

	public IEnumerable<GameVersion> Versions { get; init; } = new List<GameVersion>();

	[Sortable]
	public string DisplayName { get; init; } = "";

	[Sortable]
	public string? Abbreviation { get; init; } = "";

	[Sortable]
	public string YoutubeTags { get; init; } = "";

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
	}
}
