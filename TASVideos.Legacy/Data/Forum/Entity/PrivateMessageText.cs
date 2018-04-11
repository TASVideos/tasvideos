using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class PrivateMessageText
	{
		[Key]
		[Column("privmsgs_text_id")]
		public int Id { get; set; }

		[Column("privmsgs_text")]
		public string Text { get; set; }

		[Column("privmsgs_bbcode_uid")]
		public string BbCode_Uid { get; set; }
	}
}
