using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.GameGroups;

[RequirePermission(PermissionTo.CatalogMovies)]
public class EditModel(ApplicationDbContext db, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	public bool CanDelete { get; set; }

	[BindProperty]
	[StringLength(255)]
	public string Name { get; set; } = "";

	[BindProperty]
	[StringLength(255)]
	public string? Abbreviation { get; set; }

	[BindProperty]
	[StringLength(2000)]
	public string? Description { get; set; }

	public async Task<IActionResult> OnGet()
	{
		if (!Id.HasValue)
		{
			return Page();
		}

		var gameGroup = await db.GameGroups
			.Where(gg => gg.Id == Id.Value)
			.Select(gg => new
			{
				gg.Name,
				gg.Abbreviation,
				gg.Description
			})
			.SingleOrDefaultAsync();

		if (gameGroup is null)
		{
			return NotFound();
		}

		Name = gameGroup.Name;
		Abbreviation = gameGroup.Abbreviation;
		Description = gameGroup.Description;
		CanDelete = await CanBeDeleted();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			CanDelete = await CanBeDeleted();
			return Page();
		}

		if (Abbreviation is not null && await db.GameGroups.AnyAsync(g => g.Id != Id && g.Abbreviation == Abbreviation))
		{
			ModelState.AddModelError($"{nameof(Abbreviation)}", $"Abbreviation {Abbreviation} already exists");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var action = "created";
		GameGroup? gameGroup;
		if (Id.HasValue)
		{
			action = "updated";
			gameGroup = await db.GameGroups.FindAsync(Id.Value);
			if (gameGroup is null)
			{
				return NotFound();
			}
		}
		else
		{
			gameGroup = new GameGroup();
			db.GameGroups.Add(gameGroup);
		}

		gameGroup.Name = Name;
		gameGroup.Abbreviation = Abbreviation;
		gameGroup.Description = Description;

		var saveResult = await db.TrySaveChanges();
		SetMessage(saveResult, $"Game Group {Name} updated", $"Unable to update Game Group {Name}");
		if (saveResult.IsSuccess())
		{
			await publisher.SendGameManagement(
				$"Game Group [{gameGroup.Name}]({{0}}) {action} by {User.Name()}",
				"",
				$"GameGroups/{gameGroup.Id}");
		}

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
		SetMessage(await db.TrySaveChanges(), $"Game Group {Id} deleted", $"Unable to delete Game Group {Id}");

		return BasePageRedirect("List");
	}

	private async Task<bool> CanBeDeleted() => Id.HasValue && !await db.Games.ForGroup(Id.Value).AnyAsync();
}
