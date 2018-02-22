using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class MovieClass
	{
		[Column("movieid")]
		public int MovieId { get; set; }

		[Column("classid")]
		public int ClassId { get; set; }
	}
}
