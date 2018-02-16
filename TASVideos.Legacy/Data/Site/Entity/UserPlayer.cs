using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class UserPlayer
    {
		[Column("userid")]
		public int UserId { get; set; }

		[Column("playerid")]
		public int PlayerId { get; set; }
    }
}
