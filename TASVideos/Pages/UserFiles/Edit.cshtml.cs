namespace TASVideos.Pages.UserFiles;

public class EditModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public long Id { get; set; }

	[BindProperty]
	public UserFileEdit UserFile { get; set; } = new();

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var file = await db.UserFiles
			.Where(uf => uf.Id == Id)
			.Select(uf => new UserFileEdit
			{
				Title = uf.Title,
				Description = uf.Description ?? "",
				System = uf.SystemId,
				Game = uf.GameId,
				Hidden = uf.Hidden,
				UserId = uf.AuthorId,
				UserName = uf.Author!.UserName
			})
			.SingleOrDefaultAsync();

		if (file is null)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.EditUserFiles) && file.UserId != User.GetUserId())
		{
			return AccessDenied();
		}

		UserFile = file;

		await Initialize();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		await Initialize();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var file = await db.UserFiles.FindAsync(Id);
		if (file is null)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.EditUserFiles) && file.AuthorId != User.GetUserId())
		{
			return AccessDenied();
		}

		file.Title = UserFile.Title;
		file.Description = UserFile.Description;
		file.SystemId = UserFile.System;
		file.GameId = UserFile.Game;
		file.Hidden = UserFile.Hidden;

		SetMessage(await db.TrySaveChanges(), $"UserFile {Id} successfully updated", "Unable to update UserFile");
		return BasePageRedirect("/UserFiles/Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = (await db.GameSystems
			.ToDropDownListWithId())
			.WithDefaultEntry();

		AvailableGames = UserFile.System.HasValue
			? (await db.Games.ToDropDownList(UserFile.System.Value)).WithDefaultEntry()
			: [.. UiDefaults.DefaultEntry];
	}

	public class UserFileEdit
	{
		[StringLength(255)]
		public string Title { get; init; } = "";

		[DoNotTrim]
		public string Description { get; init; } = "";
		public int? System { get; init; }
		public int? Game { get; init; }
		public bool Hidden { get; init; }
		public int UserId { get; init; }
		public string UserName { get; init; } = "";
	}
}
