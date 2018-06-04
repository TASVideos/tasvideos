using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class MovieRating
	{
		[Column("userid")]
		public int UserId { get; set; }
		public virtual User User { get; set; }

		[Column("movieid")]
		public int MovieId { get; set; }

		[Column("ratingname")]
		public string RatingName { get; set; }

		[Column("value")]
		public decimal Value { get; set; }
	}
}
