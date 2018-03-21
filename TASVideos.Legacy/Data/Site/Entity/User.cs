using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class User
    {
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Required]
		[StringLength(255)]
		[Column("name")]
		public string Name { get; set; }

		[StringLength(255)]
		[Column("homepage")]
		public string HomePage { get; set; }

		[Column("lastchange")]
		public int LastChange { get; set; }

		[Column("createtime")]
		public int CreateTimeStamp { get; set; }

		[Column("points")]
		public double Points { get; set; }

		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
		public virtual ICollection<SiteText> SiteTexts { get; set; } = new HashSet<SiteText>();
    }
}
