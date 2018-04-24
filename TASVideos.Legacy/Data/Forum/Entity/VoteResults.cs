using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class VoteResult
	{
		[Column("vote_id")]
		public int Id { get; set; }

		[Column("vote_option_id")]
		public int VoteOptionId { get; set; }

		[Column("vote_option_text")]
		public string VoteOptionText { get; set; }

		[Column("vote_result")]
		public int ResultCount { get; set; }
	}
}
