using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class RamAddressSet
	{
		[Column("addrset")]
		public int Id { get; set; }

		[Column("name")]
		public string? Name { get; set; }

		[Column("system")]
		public int SystemId { get; set; }
	}
}
