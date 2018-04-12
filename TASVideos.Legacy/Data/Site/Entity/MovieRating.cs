using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class MovieRating
	{
		[Column("userid")]
		public int UserId { get; set; }

		[Column("movieid")]
		public int MovieId { get; set; }

		[Column("ratingname")]
		public string RatingName { get; set; }

		[Column("value")]
		public double Value { get; set; }
	}
}
