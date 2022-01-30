using TASVideos.Core;

namespace TASVideos.ViewComponents;

public record UserMaintenanceLogEntry
{
	[Sortable]
	public string User { get; init; } = "";

	[Sortable]
	public string Editor { get; init; } = "";

	[Sortable]
	public DateTime TimeStamp { get; init; }

	[Sortable]
	public string Log { get; init; } = "";
}
