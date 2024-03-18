﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Pages.AwardsEditor.Models;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class ListCategoryModel(ApplicationDbContext db, IMediaFileUploader mediaFileUploader)
	: BasePageModel
{
	public IEnumerable<AwardCategoryEntry> Categories { get; set; } = new List<AwardCategoryEntry>();

	public async Task<IActionResult> OnGet()
	{
		Categories = await db.Awards
			.Select(a => new AwardCategoryEntry
			{
				Id = a.Id,
				Type = a.Type,
				ShortName = a.ShortName,
				Description = a.Description,
				InUse = db.PublicationAwards.Any(pa => pa.AwardId == a.Id)
					|| db.UserAwards.Any(ua => ua.AwardId == a.Id)
			})
			.ToListAsync();
		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		var awardCategory = await db.Awards
			.Where(a => a.Id == id)
			.Select(a => new
			{
				a.ShortName,
				InUse = db.PublicationAwards.Any(pa => pa.AwardId == a.Id)
					|| db.UserAwards.Any(ua => ua.AwardId == a.Id)
			})
			.SingleOrDefaultAsync();
		if (awardCategory is null)
		{
			return NotFound();
		}

		if (awardCategory.InUse)
		{
			return BadRequest("Cannot delete an award category that is in use.");
		}

		db.Awards.Attach(new Award { Id = id }).State = EntityState.Deleted;
		await db.SaveChangesAsync();

		mediaFileUploader.DeleteAwardImage($"{awardCategory.ShortName}_xxxx");

		return BasePageRedirect("ListCategories");
	}
}
