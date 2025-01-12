namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	[BindProperty]
	public string Title { get; set; } = "";

	[BindProperty]
	public int? SystemId { get; set; }

	[BindProperty]
	public int? GameId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var topic = await db.ForumTopics
			.Select(t => new
			{
				t.Id,
				t.Title,
				t.GameId
			})
			.SingleOrDefaultAsync(t => t.Id == Id);
		if (topic is null)
		{
			return NotFound();
		}

		Title = topic.Title;

		if (topic.GameId.HasValue)
		{
			GameId = topic.GameId;
			SystemId = await db.GameVersions
				.Where(v => v.GameId == GameId)
				.Select(v => v.SystemId)
				.FirstOrDefaultAsync();
		}

		await Initialize();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var topic = await db.ForumTopics.FindAsync(Id);
		if (topic is null)
		{
			return NotFound();
		}

		if (GameId.HasValue)
		{
			var gameExists = GameId.HasValue && await db.Games.AnyAsync(g => g.Id == GameId);
			if (!gameExists)
			{
				return BadRequest();
			}
		}

		topic.GameId = GameId;
		SetMessage(await db.TrySaveChanges(), "Topic successfully cataloged.", "Unable to catalog topic.");

		return BasePageRedirect("Index", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = (await db.GameSystems
			.ToDropDownListWithId())
			.WithDefaultEntry();

		if (SystemId.HasValue)
		{
			AvailableGames = (await db.Games
				.ToDropDownList(SystemId.Value))
				.WithDefaultEntry();
		}
	}
}
