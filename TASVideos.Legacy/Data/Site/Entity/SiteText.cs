using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class SiteText
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Required]
		[StringLength(1)]
		[Column("type")]
		public string Type { get; set; } = "P";

		[Required]
		[Column("pagename")]
		public string PageName { get; set; }

		[Column("minoredit")]
		[StringLength(1)]
		public string MinorEdit { get; set; } = "N";

		[Required]
		[Column("whyedit")]
		public string WhyEdit { get; set; }

		[Column("revision")]
		public int Revision { get; set; } = 1;

		[Column("userid")]
		public int UserId { get; set; }
		public virtual User User { get; set; }

		[Column("timestamp")]
		public int CreateTimeStamp { get; set; }

		[Required]
		[Column("description")]
		public string Description { get; set; }
	}
}
