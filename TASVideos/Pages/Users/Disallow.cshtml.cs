using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.EditDisallows)]
public class DisallowModel(ApplicationDbContext db) : BasePageModel
{
	public List<DisallowEntry> Disallows { get; set; } = [];

	[BindProperty]
	[Display(Name = "Add New Regex Pattern")]
	public string RegexPattern { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		await PopulateDisallows();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		await PopulateDisallows();

		if (Disallows.Any(d => d.RegexPattern == RegexPattern))
		{
			ModelState.AddModelError(nameof(RegexPattern), "The provided regex pattern already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		db.UserDisallows.Add(new UserDisallow { RegexPattern = RegexPattern });
		await db.SaveChangesAsync();

		return BasePageRedirect("/Users/Disallow");
	}

	public async Task<IActionResult> OnPostDelete(int disallowId)
	{
		var disallow = await db.UserDisallows.SingleOrDefaultAsync(d => d.Id == disallowId);
		if (disallow is not null)
		{
			db.UserDisallows.Remove(disallow);
			await db.SaveChangesAsync();
		}

		return BasePageRedirect("/Users/Disallow");
	}

	private async Task PopulateDisallows()
	{
		Disallows = await db.UserDisallows
			.OrderBy(d => d.Id)
			.Select(d => new DisallowEntry(d.Id, d.RegexPattern))
			.ToListAsync();
	}

	public record DisallowEntry(int Id, string? RegexPattern);
}
