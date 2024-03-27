namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class RenderDeletedModel(ApplicationDbContext db) : BasePageModel
{
	public WikiPage WikiPage { get; set; } = null!;

	public async Task<IActionResult> OnGet(string? url, int? revision = null)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return NotFound();
		}

		var query = db.WikiPages
			.Include(wp => wp.Author)
			.ThatAreDeleted()
			.Where(wp => wp.PageName == url);

		query = revision.HasValue
			? query.Where(wp => wp.Revision == revision)
			: query.ThatAreCurrent();

		var page = await query.FirstOrDefaultAsync();
		if (page is null)
		{
			return NotFound();
		}

		WikiPage = page;

		return Page();
	}
}
