using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class RenderDeletedModel : PageModel
{
	private readonly IWikiPages _wikiPages;

	public RenderDeletedModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public WikiPage WikiPage { get; set; } = new();

	public async Task<IActionResult> OnGet(string? url, int? revision = null)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return NotFound();
		}

		var query = _wikiPages.Query
			.Include(wp => wp.Author)
			.ThatAreDeleted()
			.Where(wp => wp.PageName == url);

		if (revision.HasValue)
		{
			query = query.Where(wp => wp.Revision == revision);
		}
		else
		{
			query = query.WithNoChildren();
		}

		var page = await query.FirstOrDefaultAsync();
		if (page is null)
		{
			return NotFound();
		}

		WikiPage = page;

		return Page();
	}
}
