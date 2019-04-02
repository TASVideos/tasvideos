using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class UserRanks
	{
		[Column("user_id")]
		public int UserId { get; set; }

		[Column("rank_id")]
		public int RankId { get; set; }
	}
}
