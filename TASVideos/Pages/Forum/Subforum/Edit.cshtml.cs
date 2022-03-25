using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum;

[RequirePermission(PermissionTo.EditForums)]
public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public EditModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public ForumEditModel Forum { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var forum = await _db.Forums
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
			.Where(f => f.Id == Id)
			.Select(f => new ForumEditModel
			{
				Name = f.Name,
				Description = f.Description,
				ShortName = f.ShortName
			})
			.SingleOrDefaultAsync();

		if (forum is null)
		{
			return NotFound();
		}

		Forum = forum;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var forum = await _db.Forums
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
			.SingleOrDefaultAsync(f => f.Id == Id);

		if (forum is null)
		{
			return NotFound();
		}

		forum.Name = Forum.Name;
		forum.ShortName = Forum.ShortName;
		forum.Description = Forum.Description;

		await ConcurrentSave(_db, $"Forum {forum.Name} updated.", $"Unable to edit {forum.Name}");
		return RedirectToPage("Index", new { id = Id });
	}
}
