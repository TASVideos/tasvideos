using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TASVideos.Legacy.Data.Site.Entity;

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

		[Column("enable_html")]
		public bool EnableHtml { get; set; }

		[Column("enable_bbcode")]
		public bool EnableBbCode { get; set; }

		[ForeignKey("PosterId")]
		public virtual Users Poster { get; set; }

		[ForeignKey("LastUpdateUserId")]
		public virtual Users LastUpdateUser { get; set; }

		[ForeignKey("Id")]
		public virtual PostsText PostText { get; set; }
	}
}
