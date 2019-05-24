using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class VoteDescription
	{
		[Key]
		[Column("vote_id")]
		public int Id { get; set; }

		[Column("topic_id")]
		public int TopicId { get; set; }

		[Column("vote_text")]
		public string Text { get; set; }

		[Column("vote_start")]
		public int VoteStart { get; set; }

		[Column("vote_length")]
		public int VoteLength { get; set; }

		[ForeignKey("TopicId")]
		public virtual Topics Topic { get; set; }

		public virtual ICollection<VoteResult> Results { get; set; } = new HashSet<VoteResult>();
	}
}
