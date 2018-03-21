using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class Role
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("name")]
		public string Name { get; set; }

		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
	}
}
