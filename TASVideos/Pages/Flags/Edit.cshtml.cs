using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class EditModel : BasePageModel
{
	private readonly IFlagService _flagService;

	public ICollection<SelectListItem> AvailablePermissions { get; } = UiDefaults.DefaultEntry.Concat(PermissionUtil
		.AllPermissions()
		.Select(p => new SelectListItem
		{
			Value = ((int)p).ToString(),
			Text = p.ToString().SplitCamelCase(),
		}))
		.ToList();

	public EditModel(IFlagService flagService)
	{
		_flagService = flagService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public Flag Flag { get; set; } = new();

	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var flag = await _flagService.GetById(Id);

		if (flag == null)
		{
			return NotFound();
		}

		Flag = flag;
		InUse = await _flagService.InUse(Id);
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _flagService.Edit(Id, Flag);
		switch (result)
		{
			default:
			case FlagEditResult.Success:
				SuccessStatusMessage("Tag successfully updated.");
				return BasePageRedirect("Index");
			case FlagEditResult.NotFound:
				return NotFound();
			case FlagEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(Flag)}.{nameof(Flag.Token)}", $"{nameof(Flag.Token)} {Flag.Token} already exists");
				ClearStatusMessage();
				return Page();
			case FlagEditResult.Fail:
				ErrorStatusMessage($"Unable to delete Tag {Id}, the tag may have already been deleted or updated.");
				return Page();
		}
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var result = await _flagService.Delete(Id);
		switch (result)
		{
			case FlagDeleteResult.InUse:
				ErrorStatusMessage($"Unable to delete Flag {Id}, the tag is in use by at least 1 publication.");
				break;
			case FlagDeleteResult.Success:
				SuccessStatusMessage($"Flag {Id}, deleted successfully.");
				break;
			case FlagDeleteResult.NotFound:
				ErrorStatusMessage($"Flag {Id}, not found.");
				break;
			case FlagDeleteResult.Fail:
				ErrorStatusMessage($"Unable to delete Flag {Id}, the tag may have already been deleted or updated.");
				break;
		}

		return BaseReturnUrlRedirect("Index");
	}
}
