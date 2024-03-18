﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class EditModel(IGameSystemService systemService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SystemsResponse System { get; set; } = new();

	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var system = await systemService.GetById(Id);
		if (system is null)
		{
			return NotFound();
		}

		System = system;
		InUse = await systemService.InUse(Id);
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await systemService.Edit(Id, System.Code, System.DisplayName);

		switch (result)
		{
			default:
			case SystemEditResult.Success:
				SuccessStatusMessage("Tag successfully updated.");
				return BasePageRedirect("Index");
			case SystemEditResult.NotFound:
				return NotFound();
			case SystemEditResult.DuplicateCode:
				ModelState.AddModelError($"{nameof(System)}.{nameof(System.Code)}", $"{nameof(System.Code)} {System.Code} already exists");
				ClearStatusMessage();
				return Page();
			case SystemEditResult.Fail:
				ErrorStatusMessage($"Unable to delete System {System.Code}, the system may have already been deleted or updated.");
				return Page();
		}
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var result = await systemService.Delete(Id);
		switch (result)
		{
			case SystemDeleteResult.InUse:
				ErrorStatusMessage($"Unable to delete System {Id}, the system is currently in use");
				break;
			case SystemDeleteResult.Success:
				SuccessStatusMessage($"System {Id}, deleted successfully.");
				break;
			case SystemDeleteResult.NotFound:
				ErrorStatusMessage($"System {Id}, not found.");
				break;
			case SystemDeleteResult.Fail:
				ErrorStatusMessage($"Unable to delete System {Id}, the System may have already been deleted or updated.");
				break;
		}

		return BasePageRedirect("Index");
	}
}
