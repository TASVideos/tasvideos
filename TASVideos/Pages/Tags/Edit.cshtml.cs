using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
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

		public bool InUse { get; set; } = true;

		public async Task<IActionResult> OnGet()
		{
			Tag = await _db.Tags.SingleOrDefaultAsync(t => t.Id == Id);

			if (Tag == null)
			{
				return NotFound();
			}

			InUse = await TagInUse();

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
			catch (DbUpdateException ex)
			{
				if (ex.InnerException.Message.Contains("Cannot insert duplicate"))
				{
					ModelState.AddModelError($"{nameof(Tag)}.{nameof(Tag.Code)}", $"{nameof(Tag.Code)} {Tag.Code} already exists");
					MessageType = null;
					Message = null;
					return Page();
				}
				
				MessageType = Styles.Danger;
				Message = "Unable to edit tag due to an unknown error";
			}

			return RedirectToPage("Index");
		}

		public async Task<IActionResult> OnPostDelete()
		{
			if (!await TagInUse())
			{
				try
				{
					MessageType = Styles.Success;
					Message = $"Tag {Id}, deleted successfully.";
					_db.Tags.Attach(new Tag { Id = Id }).State = EntityState.Deleted;
					await _db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					MessageType = Styles.Danger;
					Message = $"Unable to delete Tag {Id}, the tag may have already been deleted or updated.";
				}
			}

			return RedirectToPage("Index");
		}

		private async Task<bool> TagInUse()
		{
			return await _db.PublicationTags.AnyAsync(pt => pt.TagId == Id);
		}
	}
}
