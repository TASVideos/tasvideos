using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	[Table(nameof(User))]
	public class User : IdentityUser<int>
	{
		public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

		public DateTime CreateTimeStamp { get; set; } = DateTime.UtcNow;

		public DateTime? LastLoggedInTimeStamp { get; set; }
	}
}
