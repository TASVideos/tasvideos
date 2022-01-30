using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.FlagMaintenance)]
public class CreateModel : BasePageModel
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

	public CreateModel(IFlagService flagService)
	{
		_flagService = flagService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public Flag Flag { get; set; } = new();

	public IActionResult OnGet()
	{
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _flagService.Add(Flag);
		switch (result)
		{
			default:
			case FlagEditResult.Success:
				SuccessStatusMessage("Tag successfully created.");
				return BasePageRedirect("Index");
			case FlagEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(Flag)}.{nameof(Flag.Token)}", $"{nameof(Flag.Token)} {Flag.Token} already exists");
				ClearStatusMessage();
				return Page();
			case FlagEditResult.Fail:
				ErrorStatusMessage("Unable to edit tag due to an unknown error");
				return Page();
		}
	}
}
