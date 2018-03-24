using System;
using System.Collections.Generic;
using System.Text;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPost : BaseEntity
	{
		public int Id { get; set; }

		public int? TopicId { get; set; }
		public virtual ForumTopic Topic { get; set; }

		public int PosterId { get; set; }
		//public virtual User Poster { get; set; }

		public string IpAddress { get; set; }

		public string Subject { get; set; }
		public string Text { get; set; }
	}
}
