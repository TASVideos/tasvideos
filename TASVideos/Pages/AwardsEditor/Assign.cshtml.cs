using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class AssignModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IMediaFileUploader _mediaFileUploader;
	private readonly IAwards _awards;

	public AssignModel(
		ApplicationDbContext db,
		IMediaFileUploader mediaFileUploader,
		IAwards awards)
	{
		_db = db;
		_mediaFileUploader = mediaFileUploader;
		_awards = awards;
	}

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

		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var awardExists = await _awards.CategoryExists(AwardToAssign.Award!);
		if (!awardExists)
		{
			ModelState.AddModelError("", "Award does not exist.");
			await Initialize();
			return Page();
		}

		// Do not allow the assignment of an award without an image
		var exists = _mediaFileUploader.AwardExists(AwardToAssign.Award!, Year);
		if (!exists)
		{
			ModelState.AddModelError("", "Cannot assign award because award image does not exist, please upload image first.");
			ViewData["ShowUpload"] = true;
			await Initialize();
			return Page();
		}

		if (AwardToAssign.Users.Any())
		{
			await _awards.AssignUserAward(AwardToAssign.Award!, Year, AwardToAssign.Users);
		}

		if (AwardToAssign.Publications.Any())
		{
			await _awards.AssignPublicationAward(AwardToAssign.Award!, Year, AwardToAssign.Publications);
		}

		return BasePageRedirect("Index", new { DateTime.UtcNow.Year });
	}

	public async Task<IActionResult> OnPostRevoke(string shortName)
	{
		var awardToRevoke = (await _awards
			.ForYear(Year))
			.SingleOrDefault(a => a.ShortName == shortName);

		if (awardToRevoke is null)
		{
			return BadRequest("Could not find award");
		}

		await _awards.Revoke(awardToRevoke);

		return BasePageRedirect("Index", new { Year });
	}

	private async Task Initialize()
	{
		AvailableAwardCategories = UiDefaults.DefaultEntry.Concat(await _awards.AwardCategories()
			.ToDropdown(Year)
			.ToListAsync())
			.ToList();

		AvailableUsers = await _db.Users
			.Where(u => u.Publications.Any(pa => pa.Publication!.CreateTimestamp.Year == Year))
			.OrderBy(u => u.UserName)
			.ToDropdown()
			.ToListAsync();

		AvailablePublications = await _db.Publications
			.Where(p => p.CreateTimestamp.Year == Year)
			.OrderBy(p => p.Title)
			.ToDropdown()
			.ToListAsync();
	}
}
