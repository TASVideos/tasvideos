using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public class Permision
	{
		[Required]
		public PermissionTo Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		[StringLength(200)]
		public string Description { get; set; }

		public int? GroupId { get; set; }

		public virtual ICollection<Role> Roles { get; set; } = new HashSet<Role>();
	}

	public enum PermissionTo
	{
		EditWikiPages = 1,
		EditGameResources = 2,
		EditSystemPages = 3,
		EditRoles = 4,
		EditUsers = 5
	}
}
