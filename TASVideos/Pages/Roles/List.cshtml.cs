using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class ListModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<RoleDisplayModel> Roles { get; set; } = new List<RoleDisplayModel>();

	public async Task OnGet()
	{
		Roles = await _db.Roles
			.ToRoleDisplayModel()
			.ToListAsync();
	}
}
