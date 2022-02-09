using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.EditDisallows)]
public class DisallowModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public DisallowModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<DisallowEntry> Disallows { get; set; } = new List<DisallowEntry>();

	[BindProperty]
	[Required]
	[Display(Name = "Add New Regex Pattern")]
	public string? RegexPattern { get; set; }

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

		_db.UserDisallows.Add(new UserDisallow { RegexPattern = RegexPattern! });
		await _db.SaveChangesAsync();

		return BasePageRedirect("/Users/Disallow");
	}

	public async Task<IActionResult> OnPostDelete(int disallowId)
	{
		var disallow = await _db.UserDisallows.SingleOrDefaultAsync(d => d.Id == disallowId);
		if (disallow is not null)
		{
			_db.UserDisallows.Remove(disallow);
			await _db.SaveChangesAsync();
		}

		return BasePageRedirect("/Users/Disallow");
	}

	private async Task PopulateDisallows()
	{
		Disallows = await _db.UserDisallows
			.OrderBy(d => d.Id)
			.Select(d => new DisallowEntry
			{
				Id = d.Id,
				RegexPattern = d.RegexPattern
			})
			.ToListAsync();
	}
}
