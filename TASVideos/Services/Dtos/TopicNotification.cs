namespace TASVideos.Services
{
	/// <summary>
	/// Represents a notification that a new post has been added to a topic
	/// </summary>
	public class TopicNotification
	{
		public int PostId { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; } = "";
		public int PosterId { get; set; }
	}
}
