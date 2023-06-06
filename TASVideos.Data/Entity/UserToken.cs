using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserToken : IdentityUserToken<int>
{
}
