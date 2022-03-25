using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class EditModel : BasePageModel
{
	private readonly ITagService _tagService;

	public EditModel(ITagService tagService)
	{
		_tagService = tagService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public Tag Tag { get; set; } = new();

	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var tag = await _tagService.GetById(Id);

		if (tag is null)
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
				SuccessStatusMessage("Tag successfully updated.");
				return BasePageRedirect("Index");
			case TagEditResult.NotFound:
				return NotFound();
			case TagEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(Tag)}.{nameof(Tag.Code)}", $"{nameof(Tag.Code)} {Tag.Code} already exists");
				ClearStatusMessage();
				return Page();
			case TagEditResult.Fail:
				ErrorStatusMessage($"Unable to delete Tag {Id}, the tag may have already been deleted or updated.");
				return Page();
		}
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var result = await _tagService.Delete(Id);
		switch (result)
		{
			case TagDeleteResult.InUse:
				ErrorStatusMessage($"Unable to delete Tag {Id}, the tag is in use by at least 1 publication.");
				break;
			case TagDeleteResult.Success:
				SuccessStatusMessage($"Tag {Id}, deleted successfully.");
				break;
			case TagDeleteResult.NotFound:
				ErrorStatusMessage($"Tag {Id}, not found.");
				break;
			case TagDeleteResult.Fail:
				ErrorStatusMessage($"Unable to delete Tag {Id}, the tag may have already been deleted or updated.");
				break;
		}

		return BasePageRedirect("Index");
	}
}
