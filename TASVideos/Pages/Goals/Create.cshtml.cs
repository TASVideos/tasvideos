using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Goals;

[RequirePermission(Data.Entity.PermissionTo.CatalogMovies)]
public class CreateModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public CreateModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[BindProperty]
	public Goal Goal { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (await _db.Goals.AnyAsync(g => g.DisplayName == Goal.DisplayName))
		{
			ModelState.AddModelError($"{nameof(Goal)}.{nameof(Goal.DisplayName)}", "Goal already exists.");
			return Page();
		}

		_db.Goals.Add(Goal);

		await ConcurrentSave(_db, "Goal created successfully", "Unable to create goal");
		return BasePageRedirect("Index");
	}
}
