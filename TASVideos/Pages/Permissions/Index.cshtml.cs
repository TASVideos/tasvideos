using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Permissions.Models;

namespace TASVideos.Pages.Permissions;

[Authorize]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<PermissionDisplayModel> Permissions { get; } = PermissionUtil
		.AllPermissions()
		.Select(p => new PermissionDisplayModel
		{
			Id = p,
			Name = p.ToString().SplitCamelCase(),
			Group = p.Group(),
			Description = p.Description()
		})
		.ToList();

	public async Task OnGet()
	{
		var allRoles = await _db.Roles
			.Select(r => new
			{
				r.Name,
				RolePermissionId = r.RolePermission
					.Select(p => p.PermissionId)
					.ToList()
			})
			.ToListAsync();

		foreach (var permission in Permissions)
		{
			permission.Roles = allRoles
				.Where(r => r.RolePermissionId.Any(p => p == permission.Id))
				.Select(r => r.Name);
		}
	}
}
