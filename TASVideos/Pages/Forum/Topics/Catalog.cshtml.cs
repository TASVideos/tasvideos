using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	[FromRoute]
	public int Id { get; set;  }

	public CatalogModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

	[BindProperty]
	public string Title { get; set; } = "";

	[BindProperty]
	[Required]
	public int? SystemId { get; set; }

	[BindProperty]
	[Required]
	public int? GameId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var topic = await _db.ForumTopics
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
			SystemId = await _db.GameVersions
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

		var topic = await _db.ForumTopics.SingleOrDefaultAsync(t => t.Id == Id);
		if (topic is null)
		{
			return NotFound();
		}

		var gameExists = await _db.Games.AnyAsync(g => g.Id == GameId);
		if (!gameExists)
		{
			return BadRequest();
		}

		topic.GameId = GameId;
		await ConcurrentSave(_db, "Topic successfully cataloged.", "Unable to catalog topic.");

		return BasePageRedirect("Index", new { Id });
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

		if (SystemId.HasValue)
		{
			AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
				.ForSystem(SystemId.Value)
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
