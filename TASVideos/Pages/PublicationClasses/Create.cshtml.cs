namespace TASVideos.Pages.PublicationClasses;

[RequirePermission(PermissionTo.ClassMaintenance)]
public class CreateModel(IClassService classService) : BasePageModel
{
	[BindProperty]
	public PublicationClass PublicationClass { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var (_, result) = await classService.Add(PublicationClass);
		switch (result)
		{
			default:
			case ClassEditResult.Success:
				SuccessStatusMessage("PublicationClass successfully created.");
				return BasePageRedirect("Index");
			case ClassEditResult.DuplicateName:
				ModelState.AddModelError($"{nameof(PublicationClass)}.{nameof(PublicationClass.Name)}", $"{nameof(PublicationClass.Name)} {PublicationClass.Name} already exists");
				ClearStatusMessage();
				return Page();
			case ClassEditResult.Fail:
				ErrorStatusMessage("Unable to edit publicationClass due to an unknown error");
				return Page();
		}
	}
}
