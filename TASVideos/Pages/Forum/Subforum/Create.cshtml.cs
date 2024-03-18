using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum;

[RequirePermission(PermissionTo.EditForums)]
public class CreateModel(ApplicationDbContext db) : BasePageModel
{
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

		db.Forums.Add(forum);
		await ConcurrentSave(db, $"Forum {forum.Name} created successfully.", "Unable to create forum.");

		return BasePageRedirect("Index", new { forum.Id });
	}

	private async Task Initialize()
	{
		AvailableCategories = await db.ForumCategories
			.ToDropdown()
			.ToListAsync();
	}
}
