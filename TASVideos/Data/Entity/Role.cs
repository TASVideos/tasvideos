using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	public class Role : IdentityRole<int>
	{
		public virtual ICollection<RolePermission> RolePermission { get; set; } = new HashSet<RolePermission>();

		[StringLength(200)]
		public string Description { get; set; }
	}
}