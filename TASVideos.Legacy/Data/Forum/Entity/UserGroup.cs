using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class UserGroup
	{
		[Column("group_id")]
		public int GroupId { get; set; }

		[Column("user_id")]
		public int UserId { get; set; }
	}
}
