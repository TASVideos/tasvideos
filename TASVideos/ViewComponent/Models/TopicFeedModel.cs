using System;
using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class TopicFeedModel
	{
		public string Heading { get; set; }
		public bool RightAlign { get; set; }
		public bool HideContent { get; set; }

		public IEnumerable<TopicPost> Posts { get; set; } = new List<TopicPost>();

		public class TopicPost
		{
			public int Id { get; set; }
			public string Text { get; set; }
			public string Subject { get; set; }
			public string PosterName { get; set; }
			public DateTime PostTime { get; set; }
		}
	}
}
