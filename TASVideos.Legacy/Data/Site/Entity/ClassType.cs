using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class ClassType
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }
		
		[Column("abbr")]
		public string Abbreviation { get; set; }

		[Column("positivetext")]
	    public string PositiveText { get; set; }

		[Column("negativetext")]
		public string NegativeText { get; set; }

		[Column("specific")]
		public string Specific { get; set; }

		[Column("old_id")]
		public int OldId { get; set; }
	}
}
