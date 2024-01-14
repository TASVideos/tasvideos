using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Goals.Models;

namespace TASVideos.Pages.Games.Goals;

public class ListModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ListModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[Display(Name = "Game")]
	public string GameDisplayName { get; set; } = "";

	[FromRoute]
	public int GameId { get; set; }

	[FromQuery]
	public int? GoalToEdit { get; set; }

	public List<GoalListModel> Goals { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var gameDisplayName = await _db.Games
			.Where(g => g.Id == GameId).
			Select(g => g.DisplayName)
			.SingleOrDefaultAsync();

		if (gameDisplayName is null)
		{
			return NotFound();
		}

		GameDisplayName = gameDisplayName;

		Goals = await _db.GameGoals
			.Where(gg => gg.GameId == GameId)
			.Select(gg => new GoalListModel
			{
				Id = gg.Id,
				DisplayName = gg.DisplayName,
				Publications = gg.Publications
					.Select(p => new GoalListModel.PublicationEntry(p.Id, p.Title, p.ObsoletedById.HasValue)),
				Submissions = gg.Submissions
					.Select(s => new GoalListModel.SubmissionEntry(s.Id, s.Title))
			})
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPost(string? goalToCreate)
	{
		if (string.IsNullOrWhiteSpace(goalToCreate))
		{
			ErrorStatusMessage("Cannot create empty goal");
			return BackToList();
		}

		if (await _db.GameGoals.AnyAsync(gg => gg.GameId == GameId && gg.DisplayName == goalToCreate))
		{
			ErrorStatusMessage($"Cannot create goal {goalToCreate} because it already exists.");
			return BackToList();
		}

		_db.GameGoals.Add(new GameGoal
		{
			GameId = GameId,
			DisplayName = goalToCreate
		});

		await ConcurrentSave(_db, $"Goal {goalToCreate} created successfully", $"Unable to create goal {goalToCreate}");
		return BackToList();
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

		var gameGoal = await _db.GameGoals
			.SingleOrDefaultAsync(gg => gg.Id == gameGoalId);
		if (gameGoal is null)
		{
			return NotFound();
		}

		var oldGoalName = gameGoal.DisplayName;

		if (gameGoal.DisplayName.ToLower() == newGoalName.ToLower())
		{
			gameGoal.DisplayName = newGoalName;
			await ConcurrentSave(_db, $"Goal changed from {oldGoalName} to {newGoalName} successfully", $"Unable to change goal from {oldGoalName} to {newGoalName}");
			return BackToList();
		}

		if (gameGoal.DisplayName == "baseline")
		{
			ErrorStatusMessage("Cannot edit baseline goal.");
			return BackToList();
		}

		if (await _db.GameGoals.AnyAsync(gg => gg.GameId == GameId && gg.DisplayName == newGoalName))
		{
			ErrorStatusMessage($"Cannot change goal to {newGoalName} because it already exists.");
			return BackToList();
		}

		gameGoal.DisplayName = newGoalName;

		await ConcurrentSave(_db, $"Goal changed from {oldGoalName} to {newGoalName} successfully", $"Unable to change goal from {oldGoalName} to {newGoalName}");

		// Update publication and submission titles
		if (gameGoal.DisplayName != "baseline")
		{
			var pubs = await _db.Publications.IncludeTitleTables().Where(p => p.GameGoalId == gameGoal.Id).ToListAsync();
			foreach (var pub in pubs)
			{
				pub.GenerateTitle();
			}

			var subs = await _db.Submissions.IncludeTitleTables().Where(s => s.GameGoalId == gameGoal.Id).ToListAsync();
			foreach (var sub in subs)
			{
				sub.GenerateTitle();
			}

			await _db.SaveChangesAsync();
		}

		return BackToList();
	}

	public async Task<IActionResult> OnGetDelete(int gameGroupId)
	{
		if (await _db.Publications.AnyAsync(p => p.GameGoalId == gameGroupId))
		{
			ErrorStatusMessage("Game Group can not be deleted because it is associated with one or more publications.");
			return BackToList();
		}

		if (await _db.Publications.AnyAsync(p => p.GameGoalId == gameGroupId))
		{
			ErrorStatusMessage("Game Group can not be deleted because it is associated with one or more submissions.");
			return BackToList();
		}

		_db.GameGoals.Attach(new GameGoal { Id = gameGroupId }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Game Group {gameGroupId} deleted", $"Unable to delete Game Group {gameGroupId}");

		return BackToList();
	}

	private IActionResult BackToList()
	{
		return BasePageRedirect("List", new { GameId });
	}
}
