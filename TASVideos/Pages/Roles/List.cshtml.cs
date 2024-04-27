namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public List<RoleDisplay> Roles { get; set; } = [];

	public async Task OnGet()
	{
		Roles = await db.Roles
			.ToRoleDisplayModel()
			.ToListAsync();
	}

	public record RoleDisplay(bool IsDefault, int Id, string? Name, string Description, List<PermissionTo> Permissions, List<string> Links, List<string> Users);
}
