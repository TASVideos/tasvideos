using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Permissions;

[Authorize]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<PermissionDisplayModel> Permissions { get; } = PermissionUtil
		.AllPermissions()
		.Select(p => new PermissionDisplayModel(
			p,
			p.ToString().SplitCamelCase(),
			p.Group(),
			p.Description()))
		.ToList();

	public async Task OnGet()
	{
		var allRoles = await db.Roles
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
				.Select(r => r.Name)
				.ToList();
		}
	}

	public record PermissionDisplayModel(PermissionTo Id, string Name, string Description, string Group)
	{
		public List<string> Roles { get; set; } = [];
	}
}
