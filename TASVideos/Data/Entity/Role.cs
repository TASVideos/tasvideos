using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	public class Role : IdentityRole<int>
	{
		public virtual ICollection<Permision> Permissions { get; set; } = new HashSet<Permision>();
	}
}