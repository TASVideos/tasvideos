using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.GameGroups.Models;

namespace TASVideos.Pages.GameGroups;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	public bool CanDelete { get; set; }

	[BindProperty]
	public GameGroupEditModel GameGroup { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var gameGroup = await db.GameGroups
				.Where(gg => gg.Id == Id.Value)
				.Select(gg => new GameGroupEditModel
				{
					Name = gg.Name,
					Abbreviation = gg.Abbreviation,
					Description = gg.Description
				})
				.SingleOrDefaultAsync();

			if (gameGroup is null)
			{
				return NotFound();
			}

			GameGroup = gameGroup;
			CanDelete = await CanBeDeleted();
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			CanDelete = await CanBeDeleted();
			return Page();
		}

		if (GameGroup.Abbreviation != null && await db.GameGroups.AnyAsync(g => g.Id != Id && g.Abbreviation == GameGroup.Abbreviation))
		{
			ModelState.AddModelError($"{nameof(GameGroup)}.{nameof(GameGroup.Abbreviation)}", $"Abbreviation {GameGroup.Abbreviation} already exists");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		GameGroup? gameGroup;
		if (Id.HasValue)
		{
			gameGroup = await db.GameGroups
				.Where(gg => gg.Id == Id.Value)
				.SingleOrDefaultAsync();

			if (gameGroup is null)
			{
				return NotFound();
			}

			gameGroup.Name = GameGroup.Name;
			gameGroup.Abbreviation = GameGroup.Abbreviation;
			gameGroup.Description = GameGroup.Description;
		}
		else
		{
			gameGroup = new GameGroup
			{
				Name = GameGroup.Name,
				Abbreviation = GameGroup.Abbreviation,
				Description = GameGroup.Description
			};
			db.GameGroups.Add(gameGroup);
		}

		await ConcurrentSave(db, $"Game Group {GameGroup.Name} updated", $"Unable to update Game Group {GameGroup.Name}");
		return BasePageRedirect("Index", new { gameGroup.Id });
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!await CanBeDeleted())
		{
			ErrorStatusMessage($"Unable to delete Game Group {Id}, the group is used by an existing game.");
			return BasePageRedirect("List");
		}

		db.GameGroups.Attach(new GameGroup { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(db, $"Game Group {Id} deleted", $"Unable to delete Game Group {Id}");

		return BasePageRedirect("List");
	}

	private async Task<bool> CanBeDeleted()
	{
		return Id.HasValue && !await db.Games.ForGroup(Id.Value).AnyAsync();
	}
}
