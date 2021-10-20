using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class UserPlayer
	{
		[Column("userid")]
		public int UserId { get; set; }
		public virtual User? User { get; set; }

		[Column("playerid")]
		public int PlayerId { get; set; }
		public virtual Player? Player { get; set; }
	}
}
