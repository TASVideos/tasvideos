using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<RoleDisplayModel> Roles { get; set; } = new List<RoleDisplayModel>();

	public async Task OnGet()
	{
		Roles = await db.Roles
			.ToRoleDisplayModel()
			.ToListAsync();
	}
}
