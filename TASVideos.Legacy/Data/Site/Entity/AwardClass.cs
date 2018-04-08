using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class AwardClass
    {
		[Key]
		[Column("award")]
		public int Id { get; set; }

		[Column("awardclass")]
		public string Class { get; set; }

		[Column("shortname")]
		public string ShortName { get; set; }

		[Column("description")]
		public string Description { get; set; }
    }
}
