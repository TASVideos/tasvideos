using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserRole : IdentityUserRole<int>
{
	public User? User { get; set; }
	public Role? Role { get; set; }
}
