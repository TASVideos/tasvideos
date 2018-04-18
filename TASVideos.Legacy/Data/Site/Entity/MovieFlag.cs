using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class MovieFlag
	{
		[Column("movie")]
		public int MovieId { get; set; }

		[Column("flag")]
		public int FlagId { get; set; }
	}
}
