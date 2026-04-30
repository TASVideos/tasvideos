namespace TASVideos.Pages.Wiki.Redirects;

[RequirePermission(PermissionTo.EditWikiRedirects)]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public List<WikiRedirect> Redirects { get; set; } = [];

	public async Task OnGet()
	{
		Redirects = await db.WikiRedirects
			.OrderBy(r => r.PageNameFrom)
			.ThenBy(r => r.PageNameTo)
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		var redirect = await db.WikiRedirects.FindAsync(id);
		if (redirect is null)
		{
			return NotFound();
		}

		db.WikiRedirects.Remove(redirect);
		SetMessage(await db.TrySaveChanges(), $"Redirect from '{redirect.PageNameFrom}' to '{redirect.PageNameTo}' deleted successfully", $"Unable to delete redirect from '{redirect.PageNameFrom}' to '{redirect.PageNameTo}'");

		return BasePageRedirect("Index");
	}
}
