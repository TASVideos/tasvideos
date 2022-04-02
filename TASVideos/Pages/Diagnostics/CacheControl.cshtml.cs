using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheControlModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;
	private readonly IAwards _awards;
	private readonly ICacheService _cache;

	public CacheControlModel(
		IWikiPages wikiPages,
		IAwards awards,
		ICacheService cache)
	{
		_wikiPages = wikiPages;
		_awards = awards;
		_cache = cache;
	}

	public void OnGet()
	{
	}

	public IActionResult OnGetCacheValue(string key)
	{
		var result = _cache.TryGetValue(key, out object value);
		return new JsonResult(new { value = result ? value : "Empty" });
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
