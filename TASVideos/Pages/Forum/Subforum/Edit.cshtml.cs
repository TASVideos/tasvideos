using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

	public bool CanDelete { get; set; }

	public IEnumerable<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var forum = await _db.Forums
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
			.Where(f => f.Id == Id)
			.Select(f => new ForumEditModel
			{
				Name = f.Name,
				Description = f.Description,
				ShortName = f.ShortName,
				CategoryId = f.CategoryId,
				Restricted = f.Restricted
			})
			.SingleOrDefaultAsync();

		if (forum is null)
		{
			return NotFound();
		}

		Forum = forum;
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
		forum.CategoryId = Forum.CategoryId;
		forum.Restricted = Forum.Restricted;

		await ConcurrentSave(_db, $"Forum {forum.Name} updated.", $"Unable to edit {forum.Name}");
		return RedirectToPage("Index", new { id = Id });
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!await CanBeDeleted())
		{
			return BadRequest("Cannot delete subforum that contains topics");
		}

		var subForum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == Id);
		if (subForum is null)
		{
			return NotFound();
		}

		_db.Forums.Remove(subForum);

		await ConcurrentSave(_db, $"Forum {Id} deleted successfully", $"Unable to delete Forum {Id}");

		return RedirectToPage("/Forum/Index");
	}

	private async Task Initialize()
	{
		CanDelete = await CanBeDeleted();
		AvailableCategories = await _db.ForumCategories
			.ToDropdown()
			.ToListAsync();
	}

	private async Task<bool> CanBeDeleted()
	{
		return !await _db.ForumTopics.AnyAsync(t => t.ForumId == Id);
	}
}
