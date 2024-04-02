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

	public class RoleDisplay
	{
		public bool IsDefault { get; init; }
		public int Id { get; init; }
		public string? Name { get; init; }
		public string Description { get; init; } = "";
		public List<PermissionTo> Permissions { get; init; } = [];

		[Display(Name = "Related Links")]
		public List<string> Links { get; init; } = [];

		[Display(Name = "Users with this Role")]
		public List<UserWithRole> Users { get; init; } = [];

		public record UserWithRole(int Id, string UserName);
	}
}
