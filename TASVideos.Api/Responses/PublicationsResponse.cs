namespace TASVideos.Api.Responses;

internal class PublicationsResponse
{
	[Sortable]
	public int Id { get; init; }

	[Sortable]
	public string Title { get; init; } = "";

	// Left in for backwards compatibility
	[Sortable]
	public string? Branch { get; init; } = "";
	public string Goal { get; init; } = "";
	public int GameGoalId { get; init; }

	[Sortable]
	public string? EmulatorVersion { get; init; } = "";

	[Sortable]
	public string Class { get; init; } = "";

	[Sortable]
	public string SystemCode { get; init; } = "";

	[Sortable]
	public int SubmissionId { get; init; }

	[Sortable]
	public int GameId { get; init; }

	[Sortable]
	public int GameVersionId { get; init; }

	[Sortable]
	public int? ObsoletedById { get; init; }

	[Sortable]
	public int Frames { get; init; }

	[Sortable]
	public int RerecordCount { get; init; }

	[Sortable]
	public double SystemFrameRate { get; init; }

	[Sortable]
	public string MovieFileName { get; init; } = "";

	[Sortable]
	public string? AdditionalAuthors { get; init; }

	[Sortable]
	public DateTime CreateTimestamp { get; init; }

	public IEnumerable<string> Authors { get; init; } = [];
	public IEnumerable<string> Tags { get; init; } = [];
	public IEnumerable<string> Flags { get; init; } = [];

	public IEnumerable<string> Urls { get; init; } = [];
	public IEnumerable<string> FilePaths { get; init; } = [];
}
