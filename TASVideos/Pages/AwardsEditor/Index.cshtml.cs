﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class IndexModel(IAwards awards) : BasePageModel
{
	private static readonly IEnumerable<AwardType> AwardTypes = Enum
		.GetValues(typeof(AwardType))
		.Cast<AwardType>()
		.ToList();

	[FromRoute]
	public int? Year { get; set; }

	public IEnumerable<AwardAssignment> Assignments { get; set; } = new List<AwardAssignment>();

	public IEnumerable<SelectListItem> AvailableAwardTypes { get; set; } = AwardTypes
		.Select(a => new SelectListItem
		{
			Text = a.ToString(),
			Value = ((int)a).ToString()
		});

	public async Task<IActionResult> OnGet()
	{
		if (!Year.HasValue)
		{
			return BasePageRedirect("Index", new { DateTime.UtcNow.Year });
		}

		Assignments = await awards.ForYear(Year.Value);

		return Page();
	}
}
