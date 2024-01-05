using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Goals;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public Goal Goal { get; set; } = new();
	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var goal = await _db.Goals.SingleOrDefaultAsync(g => g.Id == Id);
		if (goal is null)
		{
			return NotFound();
		}

		Goal = goal;
		InUse = await _db.GameGoals.AnyAsync(g => g.GoalId == Id);
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var goal = await _db.Goals.SingleOrDefaultAsync(g => g.Id == Id);
		if (goal is null)
		{
			return NotFound();
		}

		goal.DisplayName = Goal.DisplayName;
		await ConcurrentSave(_db, "Goal edited successfully", "Unable to edit goal");

		// TODO: this is copy pasta of Games/Goals/List
		// Update publication and submission titles
		if (goal.DisplayName != "baseline")
		{
			var pubs = await _db.Publications.IncludeTitleTables().Where(p => p.GameGoal!.GoalId == goal.Id).ToListAsync();
			foreach (var pub in pubs)
			{
				pub.GenerateTitle();
			}

			var subs = await _db.Submissions.IncludeTitleTables().Where(s => s.GameGoal!.GoalId == goal.Id).ToListAsync();
			foreach (var sub in subs)
			{
				sub.GenerateTitle();
			}

			await _db.SaveChangesAsync();
		}

		return BasePageRedirect("Index");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var goal = await _db.Goals.SingleOrDefaultAsync(g => g.Id == Id);
		if (goal is null)
		{
			return NotFound();
		}

		var inUse = await _db.GameGoals.AnyAsync(g => g.GoalId == Id);
		if (inUse)
		{
			return BadRequest("Goal is in use");
		}

		_db.Goals.Remove(goal);

		await ConcurrentSave(_db, "Goal delete successfully", "Unable to delete goal");
		return BasePageRedirect("Index");
	}
}
