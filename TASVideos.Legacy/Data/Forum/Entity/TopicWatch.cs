using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class TopicWatch
	{
		[Column("topic_id")]
		public int TopicId { get; set; }

		[Column("user_id")]
		public int UserId { get; set; }

		[Column("notify_status")]
		public bool NotifyStatus { get; set; }
	}
}
