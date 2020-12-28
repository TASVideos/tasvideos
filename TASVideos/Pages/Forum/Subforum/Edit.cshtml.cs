using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum
{
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
		public ForumEditModel Forum { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			Forum = await _db.Forums
				.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
				.Where(f => f.Id == Id)
				.Select(f => new ForumEditModel
				{
					Name = f.Name,
					Description = f.Description,
					ShortName = f.ShortName
				})
				.SingleOrDefaultAsync();

			if (Forum == null)
			{
				return NotFound();
			}

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

			if (forum == null)
			{
				return NotFound();
			}

			forum.Name = Forum.Name;
			forum.ShortName = Forum.ShortName;
			forum.Description = Forum.Description;

			// TODO: ideally we would put this message in temp data and redirect back to another page
			// This would keep them from re-posting the same data again instead of pressing back first
			// Since this is a pretty rarely used page, by high level users, we didn't initially do this logic
			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				ModelState.AddModelError("", "Unable to save, the page may have been modified, go back and try again.");
				return Page();
			}

			return RedirectToPage("Index", new { id = Id });
		}
	}
}
