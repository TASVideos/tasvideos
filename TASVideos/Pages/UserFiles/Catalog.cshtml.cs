using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public CatalogModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public long Id { get; set; }

	[BindProperty]
	public CatalogViewModel UserFile { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var userFile = await _db.UserFiles
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

		var userFile = await _db.UserFiles.SingleOrDefaultAsync(uf => uf.Id == Id);
		if (userFile is null)
		{
			return NotFound();
		}

		userFile.SystemId = UserFile.SystemId;
		userFile.GameId = UserFile.GameId;
		await ConcurrentSave(_db, "Userfile successfully updated.", "Unable to update Userfile.");

		return BasePageRedirect("Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = UiDefaults.DefaultEntry.Concat(await _db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync());

		if (UserFile.SystemId.HasValue)
		{
			AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
				.ForSystem((int)UserFile.SystemId)
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
