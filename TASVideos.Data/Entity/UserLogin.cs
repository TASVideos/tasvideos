using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserLogin : IdentityUserLogin<int>;
