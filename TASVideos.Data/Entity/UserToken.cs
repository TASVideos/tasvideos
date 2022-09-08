using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserToken : IdentityUserToken<int>
{
}
