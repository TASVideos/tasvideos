using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class RoleClaim : IdentityRoleClaim<int>
{
}
