namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.EditDisallows)]
public class DisallowModel(ApplicationDbContext db) : BasePageModel
{
	public List<DisallowEntry> Disallows { get; set; } = [];

	[BindProperty]
	public string AddNewRegexPattern { get; set; } = "";

	public async Task OnGet()
	{
		await PopulateDisallows();
	}

	public async Task<IActionResult> OnPost()
	{
		await PopulateDisallows();

		if (Disallows.Any(d => d.RegexPattern == AddNewRegexPattern))
		{
			ModelState.AddModelError(nameof(AddNewRegexPattern), "The provided regex pattern already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		db.UserDisallows.Add(new UserDisallow { RegexPattern = AddNewRegexPattern });
		await db.SaveChangesAsync();

		return BasePageRedirect("/Users/Disallow");
	}

	public async Task<IActionResult> OnPostDelete(int disallowId)
	{
		var disallow = await db.UserDisallows.FindAsync(disallowId);
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
