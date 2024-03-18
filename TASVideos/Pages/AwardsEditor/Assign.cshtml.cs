﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class AssignModel(
	ApplicationDbContext db,
	IMediaFileUploader mediaFileUploader,
	IAwards awards)
	: BasePageModel
{
	[FromRoute]
	public int Year { get; set; }

	[BindProperty]
	public AwardAssignmentModel AwardToAssign { get; set; } = new();

	public IReadOnlyCollection<SelectListItem> AvailableAwardCategories { get; set; } = new List<SelectListItem>();

	public IReadOnlyCollection<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();

	public IReadOnlyCollection<SelectListItem> AvailablePublications { get; set; } = new List<SelectListItem>();

	public async Task OnGet() => await Initialize();

	public async Task<IActionResult> OnPost()
	{
		if (!AwardToAssign.Users.Any() && !AwardToAssign.Publications.Any())
		{
			ModelState.AddModelError("", "At least one user or publication must be selected.");
		}

		if (AwardToAssign.Users.Any() && AwardToAssign.Publications.Any())
		{
			ModelState.AddModelError("", "Cannot assign both a user and a publication to an award.");
		}

		// TODO: rework this logic, CategoryExists() has almost the same query
		var type = await awards.AwardCategories()
			.Where(c => c.ShortName == AwardToAssign.Award)
			.Select(c => c.Type)
			.SingleAsync();

		if (type == AwardType.Movie && AwardToAssign.Users.Any())
		{
			ModelState.AddModelError("", "Cannot assign a publication award to a user.");
		}

		if (type == AwardType.User && AwardToAssign.Publications.Any())
		{
			ModelState.AddModelError("", "Cannot assign a user award to a publication.");
		}

		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var awardExists = await awards.CategoryExists(AwardToAssign.Award!);
		if (!awardExists)
		{
			ModelState.AddModelError("", "Award does not exist.");
			await Initialize();
			return Page();
		}

		// Do not allow the assignment of an award without an image
		var exists = mediaFileUploader.AwardExists(AwardToAssign.Award!, Year);
		if (!exists)
		{
			ModelState.AddModelError("", "Cannot assign award because award image does not exist, please upload image first.");
			ViewData["ShowUpload"] = true;
			await Initialize();
			return Page();
		}

		if (AwardToAssign.Users.Any())
		{
			await awards.AssignUserAward(AwardToAssign.Award!, Year, AwardToAssign.Users);
		}

		if (AwardToAssign.Publications.Any())
		{
			await awards.AssignPublicationAward(AwardToAssign.Award!, Year, AwardToAssign.Publications);
		}

		return BasePageRedirect("Index", new { DateTime.UtcNow.Year });
	}

	public async Task<IActionResult> OnPostRevoke(string shortName, AwardType type)
	{
		var awardToRevoke = (await awards
			.ForYear(Year))
			.SingleOrDefault(a => a.ShortName == shortName && a.Type == type);

		if (awardToRevoke is null)
		{
			return BadRequest("Could not find award");
		}

		await awards.Revoke(awardToRevoke);

		return BasePageRedirect("Index", new { Year });
	}

	private async Task Initialize()
	{
		AvailableAwardCategories = UiDefaults.DefaultEntry.Concat(await awards.AwardCategories()
			.OrderBy(c => c.Description)
			.ToDropdown(Year)
			.ToListAsync())
			.ToList();

		AvailableUsers = await db.Users
			.Where(u => u.Publications.Any(pa => pa.Publication!.CreateTimestamp.Year == Year))
			.OrderBy(u => u.UserName)
			.ToDropdown()
			.ToListAsync();

		AvailablePublications = await db.Publications
			.Where(p => p.CreateTimestamp.Year == Year)
			.OrderBy(p => p.Title)
			.ToDropdown()
			.ToListAsync();
	}
}
