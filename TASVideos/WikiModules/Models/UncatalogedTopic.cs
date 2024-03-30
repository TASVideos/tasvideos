namespace TASVideos.WikiModules.Models;

public class UncatalogedTopic
{
	public int Id { get; init; }
	public string Title { get; init; } = "";
	public int ForumId { get; init; }
	public string ForumName { get; init; } = "";
}
