using System;

namespace TASVideos.Pages.Profile.Models
{
	public class WatchedTopicEntry
	{
		public DateTime TopicCreateTimeStamp { get; set; }
		public bool IsNotified { get; set; }
		public int ForumId { get; set; }
		public string ForumTitle { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; }
	}
}
