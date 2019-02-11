using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPoll : BaseEntity
	{
		public int Id { get; set; }

		public int TopicId { get; set; }
		public virtual ForumTopic Topic { get; set; }

		[Required]
		[StringLength(500)]
		public string Question { get; set; }

		public DateTime? CloseDate { get; set; }

		public virtual ICollection<ForumPollOption> PollOptions { get; set; } = new HashSet<ForumPollOption>();
	}
}
