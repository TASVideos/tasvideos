using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserRole : IdentityUserRole<int>
{
	public virtual User? User { get; set; }
	public virtual Role? Role { get; set; }
}
