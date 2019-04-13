using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationFiles)]
	public class EditFileModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditFileModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationFileEditModel Publication { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new PublicationFileEditModel
				{
					Title = p.Title,
				})
				.SingleOrDefaultAsync();

			if (Publication == null)
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

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}
	}
}
