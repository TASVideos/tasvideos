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
		public int PosterId { get; set; }
		public virtual Users Poster { get; set; }

		[Column("topic_views")]
		public int Views { get; set; }
	}
}
