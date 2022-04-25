using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class CreateModel : BasePageModel
{
	private readonly IGameSystemService _systemService;

	public CreateModel(IGameSystemService systemService)
	{
		_systemService = systemService;
	}

	[BindProperty]
	public GameSystem System { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		System.Id = await _systemService.NextId();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _systemService.Add(System.Id, System.Code, System.DisplayName);

		switch (result)
		{
			default:
			case SystemEditResult.Success:
				SuccessStatusMessage($"System {System.Code} successfully created.");
				return BasePageRedirect("Index");
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