using System;

namespace TASVideos.Services
{
	/// <summary>
	/// Represents a watched forum topic
	/// </summary>
	public class WatchedTopic
	{
		public DateTime TopicCreateTimeStamp { get; set; }
		public bool IsNotified { get; set; }
		public int ForumId { get; set; }
		public string ForumTitle { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; }
	}
}
