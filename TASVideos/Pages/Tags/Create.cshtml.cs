using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
	public class CreateModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public CreateModel(ApplicationDbContext db)
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
		public Tag Tag { get; set; } = new Tag();

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			_db.Tags.Add(new Tag
			{
				Code = Tag.Code,
				DisplayName = Tag.DisplayName
			});

			try
			{
				MessageType = Styles.Success;
				Message = "Tag successfully created.";
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				MessageType = Styles.Danger;
				Message = "Unable to create Tag.";
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
	}
}
