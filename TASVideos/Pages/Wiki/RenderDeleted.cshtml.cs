namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class RenderDeletedModel(ApplicationDbContext db) : BasePageModel
{
	public List<WikiPage> WikiPages { get; set; } = [];

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

		var pages = await query.ToListAsync();
		if (pages.Count == 0)
		{
			return NotFound();
		}

		WikiPages = pages;
		return Page();
	}
}
