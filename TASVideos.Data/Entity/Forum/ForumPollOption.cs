using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPollOption : BaseEntity
	{
		public int Id { get; set; }

		[Required]
		public string Text { get; set; }

		public int PollId { get; set; }
		public virtual ForumPoll Poll { get; set; }

		public virtual ICollection<ForumPollOptionVote> Votes { get; set; } = new HashSet<ForumPollOptionVote>();
	}
}
