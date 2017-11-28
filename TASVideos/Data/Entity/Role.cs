using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	public class Role : IdentityRole<int>, ITrackable
	{
		[StringLength(200)]
		public string Description { get; set; }

		public DateTime CreateTimeStamp { get; set; }
		public DateTime LastUpdateTimeStamp { get; set; }

		public virtual ICollection<RolePermission> RolePermission { get; set; } = new HashSet<RolePermission>();
		public virtual ICollection<UserRole> UserRole { get; set; } = new HashSet<UserRole>();
	}
}