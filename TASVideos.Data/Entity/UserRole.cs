using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserRole : IdentityUserRole<int>
{
	public virtual User? User { get; set; }
	public virtual Role? Role { get; set; }
}
