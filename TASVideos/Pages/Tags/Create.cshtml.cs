namespace TASVideos.Pages.Tags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class CreateModel(ITagService tagService) : BasePageModel
{
	[BindProperty]
	public Tag Tag { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var (_, result) = await tagService.Add(Tag.Code, Tag.DisplayName);
		switch (result)
		{
			default:
			case TagEditResult.Success:
				SuccessStatusMessage("Tag successfully created.");
				return BasePageRedirect("Index");
			case TagEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(Tag)}.{nameof(Tag.Code)}", $"{nameof(Tag.Code)} {Tag.Code} already exists");
				ClearStatusMessage();
				return Page();
			case TagEditResult.Fail:
				ErrorStatusMessage("Unable to edit tag due to an unknown error");
				return Page();
		}
	}
}
