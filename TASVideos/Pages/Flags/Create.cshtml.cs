namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.FlagMaintenance)]
public class CreateModel(IFlagService flagService) : BasePageModel
{
	[BindProperty]
	public Flag Flag { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await flagService.Add(Flag);
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
