using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
public class RenderDeletedModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;

	public RenderDeletedModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public IWikiPage WikiPage { get; set; } = null!;

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

		query = revision.HasValue
			? query.Where(wp => wp.Revision == revision)
			: query.WithNoChildren();

		var page = await query.FirstOrDefaultAsync();
		if (page is null)
		{
			return NotFound();
		}

		WikiPage = page;

		return Page();
	}
}
