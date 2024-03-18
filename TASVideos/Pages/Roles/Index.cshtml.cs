using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public RoleDisplayModel RoleViewModel { get; set; } = new();

	[FromRoute]
	public string Role { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(Role))
		{
			return BasePageRedirect("/Roles/List");
		}

		Role = Role.Replace(" ", "");
		var roleModel = await db.Roles
			.Where(r => r.Name.Replace(" ", "") == Role)
			.ToRoleDisplayModel()
			.SingleOrDefaultAsync();

		if (roleModel is null)
		{
			return NotFound();
		}

		RoleViewModel = roleModel;
		return Page();
	}
}
