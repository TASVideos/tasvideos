using TASVideos.Data.Entity.Forum;

namespace TASVideos.Api.Responses;

public class ForumTopicResponse
{
	[Sortable]
	public int Id { get; init; }

	[Sortable]
	public int ForumId { get; init; }
	public string Title { get; init; } = "";

	[Sortable]
	public int PosterId { get; init; }

	public string Type { get; init;  } = "";

	[Sortable]
	public bool IsLocked { get; init; }
	public int? PollId { get; init; }
	public int? SubmissionId { get; init; }

	[Sortable]
	public int? GameId { get; init; }

	[Sortable]
	public DateTime CreateTimestamp { get; init; }

	[Sortable]
	public DateTime LastUpdateTimestamp { get; init; }
}
