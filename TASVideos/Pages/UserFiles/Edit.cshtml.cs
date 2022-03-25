using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditModel(
		ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public long Id { get; set; }

	[BindProperty]
	public UserFileEditModel UserFile { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var file = await _db.UserFiles
			.Where(uf => uf.Id == Id)
			.Select(uf => new UserFileEditModel
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

		var file = await _db.UserFiles
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

		await ConcurrentSave(_db, $"UserFile {Id} successfully updated", "Unable to update UserFile");
		return BasePageRedirect("/UserFiles/Info", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = UiDefaults.DefaultEntry.Concat(await _db.GameSystems
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync());

		AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
			.OrderBy(g => g.SystemId)
			.ThenBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync());
	}
}
