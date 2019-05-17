using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class GameNameGroup
	{
		[Column("gn_id")]
		public int GnId { get; set; }

		[Column("group")]
		public int GroupId { get; set; }
	}
}
