using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheControlModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;
	private readonly IAwards _awards;

	public CacheControlModel(
		IWikiPages wikiPages,
		IAwards awards)
	{
		_wikiPages = wikiPages;
		_awards = awards;
	}

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostFlushWikiCache()
	{
		await _wikiPages.FlushCache();
		return BasePageRedirect("CacheControl");
	}

	public IActionResult OnPostClearAwardsCache()
	{
		_awards.FlushCache();
		return BasePageRedirect("CacheControl");
	}
}
