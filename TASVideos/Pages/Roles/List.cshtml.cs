﻿using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public List<RoleDisplayModel> Roles { get; set; } = [];

	public async Task OnGet()
	{
		Roles = await db.Roles
			.ToRoleDisplayModel()
			.ToListAsync();
	}
}
