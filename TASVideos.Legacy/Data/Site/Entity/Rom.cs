using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class Rom
    {
		[Key]
		[Column("rom_id")]
		public int Id { get; set; }

		[Column("md5")]
		public string Md5 { get; set; }

		[Column("sha1")]
		public string Sha1 { get; set; }

		[Column("description")]
		public string Description { get; set; }

		[Column("gn_id")]
		public int GameId { get; set; }

		[Column("type")]
		public string Type { get; set; }
	}
}
