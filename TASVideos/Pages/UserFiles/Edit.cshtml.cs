using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Game;

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
				SystemId = uf.SystemId,
				GameId = uf.GameId,
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

		var file = await db.UserFiles
			.SingleOrDefaultAsync(uf => uf.Id == Id);

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
		file.SystemId = UserFile.SystemId;
		file.GameId = UserFile.GameId;
		file.Hidden = UserFile.Hidden;

		await ConcurrentSave(db, $"UserFile {Id} successfully updated", "Unable to update UserFile");
		return BasePageRedirect("/UserFiles/Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = (await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDownWithId()
			.ToListAsync())
			.WithDefaultEntry();

		if (UserFile.SystemId.HasValue)
		{
			AvailableGames =
			[
				.. AvailableGames,
				.. await db.Games
					.ForSystem(UserFile.SystemId.Value)
					.OrderBy(g => g.DisplayName)
					.ToDropDown()
					.ToListAsync()
			];
		}
		else
		{
			AvailableGames = [.. UiDefaults.DefaultEntry];
		}
	}

	public class UserFileEdit
	{
		[StringLength(255)]
		public string Title { get; init; } = "";

		[DoNotTrim]
		public string Description { get; init; } = "";

		[Display(Name = "System")]
		public int? SystemId { get; init; }

		[Display(Name = "Game")]
		public int? GameId { get; init; }

		public bool Hidden { get; init; }

		public int UserId { get; init; }

		public string UserName { get; init; } = "";
	}
}
