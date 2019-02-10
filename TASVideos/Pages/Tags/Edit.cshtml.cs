using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
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

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

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

			try
			{
				MessageType = Styles.Success;
				Message = "Tag successfully updated.";
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = $"Unable to delete Tag {Id}, the tag may have already been deleted or updated.";
			}

			return RedirectToPage("Index");
		}
	}
}
