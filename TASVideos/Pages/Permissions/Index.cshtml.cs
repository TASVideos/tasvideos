namespace TASVideos.Pages.Permissions;

[Authorize]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public List<PermissionDisplay> Permissions { get; } = PermissionUtil
		.AllPermissions()
		.Select(p => new PermissionDisplay(
			p,
			p.ToString(),
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

	public record PermissionDisplay(PermissionTo Id, string Name, string Group, string Description)
	{
		public List<string> Roles { get; set; } = [];
	}
}
