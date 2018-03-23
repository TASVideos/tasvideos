using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Posts
	{
		[Key]
		[Column("post_id")]
		public int Id { get; set; }

		[Column("topic_id")]
		public int TopicId { get; set; }

		[Column("poster_id")]
		public int PosterId { get; set; }

		[Column("post_time")]
		public int Timestamp { get; set; }

		[Column("poster_ip")]
		public string IpAddress { get; set; }

		[Column("post_edit_time")]
		public int? LastUpdateTimestamp { get; set; }

		[Column("post_edit_userid")]
		public int? LastUpdateUserId { get; set; }
	}
}
