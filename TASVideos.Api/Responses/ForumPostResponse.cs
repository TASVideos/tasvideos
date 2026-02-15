using TASVideos.Data.Entity.Forum;

namespace TASVideos.Api.Responses;

public class ForumPostResponse
{
	[Sortable]
	public int Id { get; init; }
	public int? TopicId { get; init; }
	public int ForumId { get; init; }

	[Sortable]
	public int PosterId { get; init; }
	public string? Subject { get; init; }
	public string Text { get; init; } = "";
	public DateTime? PostEditedTimestamp { get; init; }
	public bool EnableHtml { get; init; }
	public bool EnableBbCode { get; init; }
	public ForumPostMood PosterMood { get; init; }

	[Sortable]
	public DateTime CreateTimestamp { get; set; }
	public DateTime LastUpdateTimestamp { get; set; }
}
