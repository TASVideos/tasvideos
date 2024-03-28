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
		public bool IsDefault { get; set; }
		public int Id { get; set; }
		public string? Name { get; set; }
		public string Description { get; set; } = "";
		public List<PermissionTo> Permissions { get; set; } = [];

		[Display(Name = "Related Links")]
		public List<string> Links { get; set; } = [];

		[Display(Name = "Users with this Role")]
		public List<UserWithRole> Users { get; set; } = [];

		public record UserWithRole(int Id, string UserName);
	}
}
