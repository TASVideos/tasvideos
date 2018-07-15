using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class UserFileComment
	{
		[Key, Column("id")]
		public int Id { get; set; }

		[Column("file_id")]
		public long FileId { get; set; }
		public virtual UserFile File { get; set; }

		[Column("ip"), StringLength(255), Required]
		public string Ip { get; set; }

		[Column("parent")]
		public int? ParentId { get; set; }
		public virtual UserFileComment Parent { get; set; }

		[Column("title"), StringLength(255), Required]
		public string Title { get; set; }

		[Column("text"), Required]
		public string Text { get; set; }

		[Column("timestamp")]
		public long Timestamp { get; set; }

		[Column("userid")]
		public int UserId { get; set; }
		public virtual User User { get; set; }
	}
}
