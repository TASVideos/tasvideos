namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public long Id { get; set; }

	[BindProperty]
	public Catalog UserFile { get; set; } = new();

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var userFile = await db.UserFiles
			.Select(uf => new Catalog
			{
				Id = uf.Id,
				Game = uf.GameId,
				System = uf.SystemId,
				Filename = uf.FileName,
				AuthorName = uf.Author!.UserName
			})
			.SingleOrDefaultAsync(uf => uf.Id == Id);

		if (userFile is null)
		{
			return NotFound();
		}

		UserFile = userFile;
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

		var count = await db.UserFiles.Where(uf => uf.Id == Id)
			.ExecuteUpdateAsync(s => s
				.SetProperty(uf => uf.SystemId, UserFile.System)
				.SetProperty(uf => uf.GameId, UserFile.Game));

		SetMessage(count > 0, "Userfile successfully updated", "Unable to update Userfile");
		return BasePageRedirect("Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = (await db.GameSystems
			.ToDropDownListWithId())
			.WithDefaultEntry();

		if (UserFile.System.HasValue)
		{
			AvailableGames = (await db.Games
				.ToDropDownList(UserFile.System))
				.WithDefaultEntry();
		}
	}

	public class Catalog
	{
		public long Id { get; init; }

		[Required]
		public int? System { get; init; }
		public int? Game { get; init; }
		public string Filename { get; init; } = "";
		public string AuthorName { get; init; } = "";
	}
}
