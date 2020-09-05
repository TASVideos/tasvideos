using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class RamAddress
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("address")]
		public long Address { get; set; }

		[Column("datatype")]
		public string? Type { get; set; }

		[Column("signed")]
		public string? Signed { get; set; }

		[Column("endian")]
		public string? Endian { get; set; }

		[Column("description")]
		public string? Description { get; set; }

		[Column("domain")]
		public int Domain { get; set; }

		[ForeignKey("AddressSet")]
		[Column("addrset")]
		public int AddressSetId { get; set; }

		public virtual RamAddressSet? AddressSet { get; set; }
	}
}
