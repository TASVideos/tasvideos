using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class RamAddressDomain
	{
		[Key]
		[Column("domain")]
		public int Id { get; set; }

		[Column("name")]
		public string? Name { get; set; }

		[Column("system")]
		public int? SystemId { get; set; }
	}
}
