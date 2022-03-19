using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.GameGroups.Models;

namespace TASVideos.Pages.GameGroups;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int? Id { get; set; }

	public bool CanDelete { get; set; }

	[BindProperty]
	public GameGroupEditModel GameGroup { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var gameGroup = await _db.GameGroups
				.Where(gg => gg.Id == Id.Value)
				.Select(gg => new GameGroupEditModel
				{
					Name = gg.Name,
					SearchKey = gg.SearchKey
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

		GameGroup? gameGroup;
		if (Id.HasValue)
		{
			gameGroup = await _db.GameGroups
				.Where(gg => gg.Id == Id.Value)
				.SingleOrDefaultAsync();

			if (gameGroup is null)
			{
				return NotFound();
			}

			gameGroup.Name = GameGroup.Name;
			gameGroup.SearchKey = GameGroup.SearchKey;
		}
		else
		{
			gameGroup = new GameGroup
			{
				Name = GameGroup.Name,
				SearchKey = GameGroup.SearchKey
			};
			_db.GameGroups.Add(gameGroup);
		}

		await ConcurrentSave(_db, $"Game Group {GameGroup.Name} updated", $"Unable to update Game Group {GameGroup.Name}");
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

		_db.GameGroups.Attach(new GameGroup { Id = Id ?? 0 }).State = EntityState.Deleted;
		await ConcurrentSave(_db, $"Game Group {Id} deleted", $"Unable to delete Game Group {Id}");

		return BasePageRedirect("List");
	}

	private async Task<bool> CanBeDeleted()
	{
		return Id.HasValue && !await _db.Games.ForGroup(Id.Value).AnyAsync();
	}
}
