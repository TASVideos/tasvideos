using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class Awards
    {
		[Column("UserID")]
		public int UserId { get; set; }
		public virtual User User { get; set; }

		[Column("MovieId")]
		public int MovieId { get; set; }

		[Column("Award")]
		public int AwardId { get; set; }

		[Column("Year")]
		public int Year { get; set; }
	}
}
