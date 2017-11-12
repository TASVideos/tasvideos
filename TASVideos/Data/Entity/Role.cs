using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	public class Role : IdentityRole<int>
	{
		[StringLength(200)]
		public string Description { get; set; }

		public virtual ICollection<RolePermission> RolePermission { get; set; } = new HashSet<RolePermission>();
		public virtual ICollection<UserRole> UserRole { get; set; } = new HashSet<UserRole>();
	}
}