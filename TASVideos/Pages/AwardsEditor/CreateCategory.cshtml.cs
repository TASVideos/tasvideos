﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Pages.AwardsEditor.Models;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class CreateCategoryModel(IAwards awards, IMediaFileUploader mediaFileUploader) : BasePageModel
{
	private static readonly IEnumerable<AwardType> AwardTypes = Enum
		.GetValues(typeof(AwardType))
		.Cast<AwardType>()
		.ToList();

	public IEnumerable<SelectListItem> AvailableAwardTypes { get; set; } = AwardTypes
		.Select(a => new SelectListItem
		{
			Text = a.ToString(),
			Value = ((int)a).ToString()
		});

	[BindProperty]
	public CreateAwardCategoryModel AwardCategory { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!AwardCategory.BaseImage!.FileName.EndsWith(".png"))
		{
			ModelState.AddModelError($"{nameof(AwardCategory)}.{nameof(AwardCategory.BaseImage)}", "Invalid image type, must be .png");
			return Page();
		}

		await mediaFileUploader.UploadAwardImage(
			AwardCategory.BaseImage!,
			AwardCategory.BaseImage2X!,
			AwardCategory.BaseImage4X!,
			AwardCategory.ShortName);

		var result = await awards.AddAwardCategory(
			AwardCategory.Type,
			AwardCategory.ShortName,
			AwardCategory.Description);

		if (!result)
		{
			ModelState.AddModelError("", "An award with this short name and description already exist");
		}

		return BasePageRedirect("Index");
	}
}
