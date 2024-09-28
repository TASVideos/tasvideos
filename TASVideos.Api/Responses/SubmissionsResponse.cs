namespace TASVideos.Api.Responses;

internal class SubmissionsResponse
{
	[Sortable]
	public int Id { get; init; }

	[Sortable]
	public int? PublicationId { get; init; }

	[Sortable]
	public string Title { get; init; } = "";

	[Sortable]
	public string? IntendedClass { get; init; }

	[Sortable]
	public string? Judge { get; init; }

	[Sortable]
	public string? Publisher { get; init; }

	[Sortable]
	public string Status { get; init; } = "";

	[Sortable]
	public string? MovieExtension { get; init; }

	[Sortable]
	public int? GameId { get; init; }

	[Sortable]
	public string? GameName { get; init; }

	[Sortable]
	public int? GameVersionId { get; init; }

	[Sortable]
	public string? GameVersion { get; init; }

	[Sortable]
	public string? SystemCode { get; init; }

	[Sortable]
	public double? SystemFrameRate { get; init; }

	[Sortable]
	public int Frames { get; init; }

	[Sortable]
	public int RerecordCount { get; init; }

	[Sortable]
	public string? EncodeEmbedLink { get; init; }

	[Sortable]
	public string? Branch { get; init; }
	public string? Goal { get; init; }

	[Sortable]
	public string? RomName { get; init; }

	[Sortable]
	public string? EmulatorVersion { get; init; }

	[Sortable]
	public int? MovieStartType { get; init; }

	[Sortable]
	public string? AdditionalAuthors { get; init; }

	public IEnumerable<string> Authors { get; init; } = [];

	[Sortable]
	public DateTime CreateTimestamp { get; init; }

	[Sortable]
	public DateTime? SyncedOn { get; init; }

	[Sortable]
	public string? SyncedByUser { get; init; }
}
