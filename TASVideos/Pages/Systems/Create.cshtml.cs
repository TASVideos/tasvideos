using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class CreateModel(IGameSystemService systemService) : BasePageModel
{
	[BindProperty]
	public GameSystem System { get; set; } = new();

	public async Task OnGet()
	{
		System.Id = await systemService.NextId();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await systemService.Add(System.Id, System.Code, System.DisplayName);

		switch (result)
		{
			default:
			case SystemEditResult.Success:
				SuccessStatusMessage($"System {System.Code} successfully created.");
				return BasePageRedirect("Edit", new { System.Id });
			case SystemEditResult.DuplicateId:
				ModelState.AddModelError($"{nameof(System)}.{nameof(System.Id)}", $"{nameof(System.Id)} {System.Id} already exists");
				ClearStatusMessage();
				return Page();
			case SystemEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(System)}.{nameof(System.Code)}", $"{nameof(System.Code)} {System.Code} already exists");
				ClearStatusMessage();
				return Page();
			case SystemEditResult.Fail:
				ErrorStatusMessage("Unable to edit tag due to an unknown error");
				return Page();
		}
	}
}
