using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity
{
	[Table(nameof(User))]
	public class User : IdentityUser<int>
	{
	}
}
