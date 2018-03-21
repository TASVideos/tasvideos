using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class UserRole
    {
		[Column("user")]
		public int UserId { get; set; }
		public virtual User User { get; set; }

		[Column("role")]
		public int RoleId { get; set; }
		public virtual Role Role { get; set; }
    }
}
