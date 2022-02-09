using TASVideos.Core;

#pragma warning disable 1591

namespace TASVideos.Api.Responses;

public class SubmissionsResponse
{
	[Sortable]
	public int Id { get; init; }

	[Sortable]
	public string Title { get; init; } = "";

	[Sortable]
	public string IntendedClass { get; init; } = "";

	[Sortable]
	public string Judge { get; init; } = "";

	[Sortable]
	public string Publisher { get; init; } = "";

	[Sortable]
	public string Status { get; init; } = "";

	[Sortable]
	public string MovieExtension { get; init; } = "";

	[Sortable]
	public int? GameId { get; init; }

	[Sortable]
	public int? RomId { get; init; }

	[Sortable]
	public string SystemCode { get; init; } = "";

	[Sortable]
	public double? SystemFrameRate { get; init; }

	[Sortable]
	public int Frames { get; init; }

	[Sortable]
	public int RerecordCount { get; init; }

	[Sortable]
	public string EncodeEmbedLink { get; init; } = "";

	[Sortable]
	public string GameVersion { get; init; } = "";

	[Sortable]
	public string GameName { get; init; } = "";

	[Sortable]
	public string Branch { get; init; } = "";

	[Sortable]
	public string RomName { get; init; } = "";

	[Sortable]
	public string EmulatorVersion { get; init; } = "";

	[Sortable]
	public int? MovieStartType { get; init; }

	public IEnumerable<string> Authors { get; init; } = new List<string>();
}
