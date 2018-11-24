using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class UserFile
	{
		[Key, Column("file_id")]
		public long Id { get; set; }

		[Column("file_uid")]
		public int UserId { get; set; }
		public virtual User User { get; set; }

		[Column("file_name"), StringLength(255), Required]
		public string Name { get; set; }

		[Column("file_content"), Required]
		public byte[] Content { get; set; }

		[Column("file_class"), StringLength(1), Required]
		public string Class { get; set; }

		[Column("file_type"), StringLength(16), Required]
		public string Type { get; set; }

		[Column("file_ts")]
		public long Timestamp { get; set; }

		[Column("file_system")]
		public int? SystemId { get; set; }

		[Column("file_length")]
		public decimal Length { get; set; }

		[Column("file_frames")]
		public int Frames { get; set; }

		[Column("file_rerecords")]
		public long Rerecords { get; set; }

		[Column("file_title"), StringLength(255), Required]
		public string Title { get; set; }

		[Column("file_description"), Required]
		public string Description { get; set; }

		[Column("file_log_len")]
		public int LogicalLength { get; set; }

		[Column("file_phys_len")]
		public int PhysicalLength { get; set; }

		[Column("file_gn_id")]
		public int? GameNameId { get; set; }
		public virtual GameName GameName { get; set; }

		[Column("file_hidden")]
		public sbyte Hidden { get; set; }

		[Column("file_warnings"), Required]
		public string Warnings { get; set; }

		[Column("file_views")]
		public int Views { get; set; }

		[Column("file_downloads")]
		public int Downloads { get; set; }
	}
}
