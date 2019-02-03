using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class BanList
	{
		[Key]
		[Column("ban_id")]
		public int Id { get; set; }

		[Column("ban_userid")]
		public int UserId { get; set; }

		[Column("ban_ip")]
		public string IpAddress { get; set; }
	}
}
