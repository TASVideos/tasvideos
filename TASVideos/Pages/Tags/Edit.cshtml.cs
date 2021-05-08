using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
	public class EditModel : BasePageModel
	{
		private readonly ITagService _tagService;

		public EditModel(ITagService tagService)
		{
			_tagService = tagService;
		}

		[TempData]
		public string? Message { get; set; }

		[TempData]
		public string? MessageType { get; set; }

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public Tag Tag { get; set; } = new ();

		public bool InUse { get; set; } = true;

		public async Task<IActionResult> OnGet()
		{
			var tag = await _tagService.GetById(Id);

			if (tag == null)
			{
				return NotFound();
			}

			Tag = tag;
			InUse = await _tagService.InUse(Id);
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var result = await _tagService.Edit(Id, Tag.Code, Tag.DisplayName);
			switch (result)
			{
				default:
				case TagEditResult.Success:
					MessageType = Styles.Success;
					Message = "Tag successfully updated.";
					return RedirectToPage("Index");
				case TagEditResult.NotFound:
					return NotFound();
				case TagEditResult.DuplicateCode:
					ModelState.AddModelError($"{nameof(Tag)}.{nameof(Tag.Code)}", $"{nameof(Tag.Code)} {Tag.Code} already exists");
					MessageType = null;
					Message = null;
					return Page();
				case TagEditResult.Fail:
					MessageType = Styles.Danger;
					Message = $"Unable to delete Tag {Id}, the tag may have already been deleted or updated.";
					return Page();
			}
		}

		public async Task<IActionResult> OnPostDelete()
		{
			var result = await _tagService.Delete(Id);
			switch (result)
			{
				case TagDeleteResult.InUse:
					MessageType = Styles.Danger;
					Message = $"Unable to delete Tag {Id}, the tag is in use by at least 1 publication.";
					break;
				case TagDeleteResult.Success:
					MessageType = Styles.Success;
					Message = $"Tag {Id}, deleted successfully.";
					break;
				case TagDeleteResult.NotFound:
					MessageType = Styles.Danger;
					Message = $"Tag {Id}, not found.";
					break;
				case TagDeleteResult.Fail:
					MessageType = Styles.Danger;
					Message = $"Unable to delete Tag {Id}, the tag may have already been deleted or updated.";
					break;
			}

			return RedirectToPage("Index");
		}
	}
}
