using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Voter
	{
		[Column("vote_id")]
		public int Id { get; set; }

		[Column("vote_user_id")]
		public int UserId { get; set; }

		[Column("vote_user_ip")]
		public string IpAddress { get; set; }

		[Column("vote_opt_id")]
		public int OptionId { get; set; }
	}
}
