using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CatalogMovies)]
public class CatalogModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set;  }

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = [];

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = [];

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

		var topic = await db.ForumTopics.SingleOrDefaultAsync(t => t.Id == Id);
		if (topic is null)
		{
			return NotFound();
		}

		var gameExists = await db.Games.AnyAsync(g => g.Id == GameId);
		if (!gameExists)
		{
			return BadRequest();
		}

		topic.GameId = GameId;
		await ConcurrentSave(db, "Topic successfully cataloged.", "Unable to catalog topic.");

		return BasePageRedirect("Index", new { Id });
	}

	private async Task Initialize()
	{
		AvailableSystems = UiDefaults.DefaultEntry.Concat(await db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync());

		if (SystemId.HasValue)
		{
			AvailableGames = UiDefaults.DefaultEntry.Concat(await db.Games
				.ForSystem(SystemId.Value)
				.OrderBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
