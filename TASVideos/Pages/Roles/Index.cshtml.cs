namespace TASVideos.Pages.Roles;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public ListModel.RoleDisplay RoleView { get; set; } = null!;

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

		RoleView = roleModel;
		return Page();
	}
}
