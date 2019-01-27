using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Subforum
{
	[RequirePermission(PermissionTo.EditForums)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditModel(
			ApplicationDbContext db,
			UserManager userManager)
			: base(userManager)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public ForumEditModel Forum { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Forum = await _db.Forums
				.ExcludeRestricted(UserHas(PermissionTo.SeeRestrictedForums))
				.Where(f => f.Id == Id)
				.Select(f => new ForumEditModel
				{
					Name = f.Name,
					Description = f.Description,
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
				.ExcludeRestricted(UserHas(PermissionTo.SeeRestrictedForums))
				.SingleOrDefaultAsync(f => f.Id == Id);

			if (forum == null)
			{
				return NotFound();
			}

			return RedirectToPage("Index", new { id = Id });
		}
	}
}
