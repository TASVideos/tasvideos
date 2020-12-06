using System;

namespace TASVideos.Services
{
	/// <summary>
	/// Represents a watched forum topic
	/// </summary>
	public class WatchedTopic
	{
		public DateTime TopicCreateTimeStamp { get; init; }
		public bool IsNotified { get; init; }
		public int ForumId { get; init; }
		public string ForumTitle { get; init; } = "";
		public int TopicId { get; init; }
		public string TopicTitle { get; init; } = "";
	}
}
