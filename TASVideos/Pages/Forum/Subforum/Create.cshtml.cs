using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum;

[RequirePermission(PermissionTo.EditForums)]
public class CreateModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public CreateModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[BindProperty]
	public ForumEditModel Forum { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
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

		var forum = new Data.Entity.Forum.Forum
		{
			Name = Forum.Name,
			ShortName = Forum.ShortName,
			Description = Forum.Description,
			CategoryId = Forum.CategoryId,
			Restricted = Forum.Restricted
		};

		_db.Forums.Add(forum);
		await ConcurrentSave(_db, $"Forum {forum.Name} created successfully.", "Unable to create forum.");

		return BasePageRedirect("Index", new { forum.Id });
	}

	private async Task Initialize()
	{
		AvailableCategories = await _db.ForumCategories
			.ToDropdown()
			.ToListAsync();
	}
}