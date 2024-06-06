namespace TASVideos.Pages.Forum.Subforum;

[RequirePermission(PermissionTo.EditForums)]
public class CreateModel(ApplicationDbContext db) : BasePageModel
{
	[BindProperty]
	public EditModel.ForumEdit Forum { get; set; } = new();

	public List<SelectListItem> AvailableCategories { get; set; } = [];

	public async Task OnGet() => await Initialize();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var forum = new Data.Entity.Forum.Forum
		{
			Name = Forum.Name,
			ShortName = Forum.ShortName,
			Description = Forum.Description,
			CategoryId = Forum.Category,
			Restricted = Forum.RestrictedAccess
		};

		db.Forums.Add(forum);
		SetMessage(await db.TrySaveChanges(), $"Forum {forum.Name} created successfully.", "Unable to create forum.");

		return BasePageRedirect("Index", new { forum.Id });
	}

	private async Task Initialize()
	{
		AvailableCategories = await db.ForumCategories.ToDropdownList();
	}
}
