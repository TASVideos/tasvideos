using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Users
	{
		[Key]
		[Column("user_id")]
		public int UserId { get; set; }

		[Column("user_active")]
		public bool IsActive { get; set; }

		[Column("username")]
		public string UserName { get; set; }

		[Column("user_password")]
		public string Password { get; set; }

		[Column("user_regdate")]
		public int RegDate { get; set; }

		[Column("user_emailtime")]
		public int? EmailTime { get; set; }

		[Column("user_email")]
		public string Email { get; set; }
    }
}
