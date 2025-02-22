using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games.Goals;

public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public string Game { get; set; } = "";

	[FromRoute]
	public int GameId { get; set; }

	[FromQuery]
	public int? GoalToEdit { get; set; }

	public List<GoalEntry> Goals { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var gameDisplayName = await db.Games
			.Where(g => g.Id == GameId)
			.Select(g => g.DisplayName)
			.SingleOrDefaultAsync();

		if (gameDisplayName is null)
		{
			return NotFound();
		}

		Game = gameDisplayName;

		Goals = await db.GameGoals
			.Where(gg => gg.GameId == GameId)
			.Select(gg => new GoalEntry(
				gg.Id,
				gg.DisplayName,
				gg.Publications.Select(p => new PublicationEntry(p.Id, p.Title, p.ObsoletedById.HasValue)).ToList(),
				gg.Submissions.Select(s => new SubmissionEntry(s.Id, s.Title)).ToList()))
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPost(string? goalToCreate)
	{
		if (!User.Has(PermissionTo.CatalogMovies))
		{
			return AccessDenied();
		}

		if (string.IsNullOrWhiteSpace(goalToCreate))
		{
			ErrorStatusMessage("Cannot create empty goal");
			return BackToList();
		}

		if (await db.GameGoals.AnyAsync(gg => gg.GameId == GameId && gg.DisplayName == goalToCreate))
		{
			ErrorStatusMessage($"Cannot create goal {goalToCreate} because it already exists.");
			return BackToList();
		}

		var gameGoal = new GameGoal
		{
			GameId = GameId,
			DisplayName = goalToCreate
		};

		db.GameGoals.Add(gameGoal);

		SetMessage(await db.TrySaveChanges(), $"Goal {goalToCreate} created successfully", $"Unable to create goal {goalToCreate}");
		return string.IsNullOrWhiteSpace(Request.ReturnUrl())
			? RedirectToPage("List", new { GameId })
			: BaseReturnUrlRedirect(new() { ["GameGoalId"] = gameGoal.Id.ToString() });
	}

	public async Task<IActionResult> OnPostEdit(int gameGoalId, string? newGoalName)
	{
		if (!User.Has(PermissionTo.CatalogMovies))
		{
			return AccessDenied();
		}

		if (string.IsNullOrWhiteSpace(newGoalName))
		{
			ErrorStatusMessage("Cannot create empty goal");
			return BackToList();
		}

		var gameGoal = await db.GameGoals.FindAsync(gameGoalId);
		if (gameGoal is null)
		{
			return NotFound();
		}

		if (gameGoal.DisplayName == "baseline")
		{
			ErrorStatusMessage("Cannot edit baseline goal.");
			return BackToList();
		}

		if (await db.GameGoals.AnyAsync(gg => gg.GameId == GameId && gg.DisplayName == newGoalName && gg.Id != gameGoal.Id))
		{
			ErrorStatusMessage($"Cannot change goal to {newGoalName} because it already exists.");
			return BackToList();
		}

		var oldGoalName = gameGoal.DisplayName;

		gameGoal.DisplayName = newGoalName;

		SetMessage(await db.TrySaveChanges(), $"Goal changed from {oldGoalName} to {newGoalName} successfully", $"Unable to change goal from {oldGoalName} to {newGoalName}");

		// Update publication and submission titles
		if (gameGoal.DisplayName != "baseline")
		{
			var pubs = await db.Publications.IncludeTitleTables().Where(p => p.GameGoalId == gameGoal.Id).ToListAsync();
			foreach (var pub in pubs)
			{
				pub.GenerateTitle();
			}

			var subs = await db.Submissions.IncludeTitleTables().Where(s => s.GameGoalId == gameGoal.Id).ToListAsync();
			foreach (var sub in subs)
			{
				sub.GenerateTitle();
			}

			await db.SaveChangesAsync();
		}

		return BackToList();
	}

	public async Task<IActionResult> OnGetDelete(int gameGoalId)
	{
		if (!User.Has(PermissionTo.CatalogMovies))
		{
			return AccessDenied();
		}

		var gameGoal = await db.GameGoals.FindAsync(gameGoalId);
		if (gameGoal is null)
		{
			return NotFound();
		}

		if (gameGoal.DisplayName == "baseline")
		{
			ErrorStatusMessage("Cannot delete baseline goal.");
			return BackToList();
		}

		if (await db.Publications.AnyAsync(p => p.GameGoalId == gameGoalId))
		{
			ErrorStatusMessage("Game Goal can not be deleted because it is associated with one or more publications.");
			return BackToList();
		}

		if (await db.Submissions.AnyAsync(p => p.GameGoalId == gameGoalId))
		{
			ErrorStatusMessage("Game Goal can not be deleted because it is associated with one or more submissions.");
			return BackToList();
		}

		db.GameGoals.Remove(gameGoal);
		SetMessage(await db.TrySaveChanges(), $"Game Goal {gameGoalId} deleted", $"Unable to delete Game Group {gameGoalId}");

		return BackToList();
	}

	private IActionResult BackToList() => BasePageRedirect("List", new { GameId });

	public record GoalEntry(int Id, string Name, List<PublicationEntry> Publications, List<SubmissionEntry> Submissions);

	public record PublicationEntry(int Id, string Title, bool Obs);

	public record SubmissionEntry(int Id, string Title);
}
