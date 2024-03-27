using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public long Id { get; set; }

	[BindProperty]
	public CatalogViewModel UserFile { get; set; } = new();

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var userFile = await db.UserFiles
			.Select(uf => new CatalogViewModel
			{
				Id = uf.Id,
				GameId = uf.GameId,
				SystemId = uf.SystemId,
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

		var userFile = await db.UserFiles.SingleOrDefaultAsync(uf => uf.Id == Id);
		if (userFile is null)
		{
			return NotFound();
		}

		userFile.SystemId = UserFile.SystemId;
		userFile.GameId = UserFile.GameId;
		await ConcurrentSave(db, "Userfile successfully updated.", "Unable to update Userfile.");

		return BasePageRedirect("Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems =
		[
			.. UiDefaults.DefaultEntry,
			.. await db.GameSystems
				.OrderBy(s => s.Code)
				.ToDropDownWithId()
				.ToListAsync(),
		];

		if (UserFile.SystemId.HasValue)
		{
			AvailableGames =
			[
				.. UiDefaults.DefaultEntry,
				.. await db.Games
					.ForSystem((int)UserFile.SystemId)
					.OrderBy(g => g.DisplayName)
					.ToDropDown()
					.ToListAsync(),
			];
		}
	}

	public class CatalogViewModel
	{
		public long Id { get; init; }

		[Required]
		[Display(Name = "System")]
		public int? SystemId { get; init; }

		[Display(Name = "Game")]
		public int? GameId { get; init; }

		public string Filename { get; init; } = "";
		public string AuthorName { get; init; } = "";
	}
}
