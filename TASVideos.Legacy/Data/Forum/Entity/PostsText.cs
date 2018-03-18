using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class PostsText
	{
		[Key]
		[Column("post_id")]
		public int Id { get; set; }

		[Column("bbcode_uid")]
		public string BbCodeUid { get; set; }

		[Column("post_subject")]
		public string Subject { get; set; }

		[Column("post_text")]
		public string Text { get; set; }
	}
}
