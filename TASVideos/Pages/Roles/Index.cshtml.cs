using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public RoleDisplayModel Role { get; set; } = new();

	public async Task<IActionResult> OnGet(string role)
	{
		if (string.IsNullOrWhiteSpace(role))
		{
			return BasePageRedirect("/Roles/List");
		}

		role = role.Replace(" ", "");
		var roleModel = await _db.Roles
			.Where(r => r.Name.Replace(" ", "") == role)
			.ToRoleDisplayModel()
			.SingleOrDefaultAsync();

		if (roleModel is null)
		{
			return NotFound();
		}

		Role = roleModel;
		return Page();
	}
}
