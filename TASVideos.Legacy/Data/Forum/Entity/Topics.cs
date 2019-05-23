using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Topics
	{
		[Key]
		[Column("topic_id")]
		public int Id { get; set; }

		[Column("forum_id")]
		public int ForumId { get; set; }

		[Column("topic_title")]
		public string Title { get; set; }

		[Column("topic_time")]
		public int Timestamp { get; set; }

		[Column("topic_poster")]
		public int? PosterId { get; set; }
		public virtual Users Poster { get; set; }

		[Column("topic_views")]
		public int Views { get; set; }

		[Column("topic_type")]
		public int Type { get; set; }

		[Column("topic_moved_id")]
		public int TopicMovedId { get; set; }

		// Topic status
		// define('TOPIC_UNLOCKED', 0);
		// define('TOPIC_LOCKED', 1);
		// define('TOPIC_MOVED', 2);
		[Column("topic_status")]
		public int TopicStatus { get; set; }

		[Column("submissionid")]
		public int SubmissionId { get; set; }

		public virtual VoteDescription Poll { get; set; }
	}
}
