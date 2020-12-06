namespace TASVideos.Services
{
	/// <summary>
	/// Represents a notification that a new post has been added to a topic
	/// </summary>
	public class TopicNotification
	{
		public int PostId { get; init; }
		public int TopicId { get; init; }
		public string TopicTitle { get; init; } = "";
		public int PosterId { get; init; }
	}
}
