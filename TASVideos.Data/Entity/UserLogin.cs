using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserLogin : IdentityUserLogin<int>
{
}
