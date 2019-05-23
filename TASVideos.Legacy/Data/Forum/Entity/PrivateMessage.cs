using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class PrivateMessage
	{
		[Key]
		[Column("privmsgs_id")]
		public int Id { get; set; }

		[Column("privmsgs_type")]
		public int Type { get; set; }

		[Column("privmsgs_subject")]
		public string Subject { get; set; }

		[Column("privmsgs_from_userid")]
		public int FromUserId { get; set; }

		[Column("privmsgs_to_userid")]
		public int ToUserId { get; set; }

		[Column("privmsgs_date")]
		public int Timestamp { get; set; }

		[Column("privmsgs_ip")]
		public string IpAddress { get; set; }

		[Column("privmsgs_enable_bbcode")]
		public bool EnableBbCode { get; set; }

		[Column("privmsgs_enable_html")]
		public bool EnableHtml { get; set; }

		[ForeignKey("Id")]
		public virtual PrivateMessageText PrivateMessageText { get; set; }

		[ForeignKey("FromUserId")]
		public virtual Users FromUser { get; set; }
	}
}
