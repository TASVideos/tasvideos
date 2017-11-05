using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
    public class User : IdentityUser<int>
    {
		 public ICollection<IdentityUserRole<int>> UserRoles { get; set; }
    }

	//// Cross table for a many-to-many relationship between Users and Roles
	//// Unforutnatley this is necessary in EF Core 2.0 (Unlike EF 6). Hopefully future iterations will allow us to remove this junko
	//public class UserRole : IdentityUserRole<int>
	//{
	////	public int UserId { get; set; }
	////	public User User { get; set; }

	////	public int RoleId { get; set; }
	////	public Role Role { get; set; }
	//}
}
