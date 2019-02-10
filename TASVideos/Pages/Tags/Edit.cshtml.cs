using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
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
		public Tag Tag { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Tag = await _db.Tags.SingleOrDefaultAsync(t => t.Id == Id);

			if (Tag == null)
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

			var tag = await _db.Tags.SingleOrDefaultAsync(t => t.Id == Id);

			if (tag == null)
			{
				return NotFound();
			}

			tag.Code = Tag.Code;
			tag.DisplayName = Tag.DisplayName;

			// TODO: Catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("Index");
		}
	}
}
