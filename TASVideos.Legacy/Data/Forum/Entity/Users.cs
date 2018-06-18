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

		[Column("user_posts")]
		public int PostCount { get; set; }

		[Column("user_avatar")]
		public string Avatar { get; set; }

		[Column("user_from")]
		public string From { get; set; }

		[Column("user_sig")]
		public string Signature { get; set; }

		[Column("user_permit_ratingshow")]
		public bool PublicRatings { get; set; }

		[Column("user_lastvisit")]
		public int LastVisitDate { get; set; }

		[Column("user_timezone")]
		public decimal TimeZoneOffset { get; set; }
	}
}
