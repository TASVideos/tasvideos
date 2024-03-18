﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheControlModel(
	IWikiPages wikiPages,
	IAwards awards,
	ICacheService cache)
	: BasePageModel
{
	public void OnGet()
	{
	}

	public IActionResult OnGetCacheValue(string key)
	{
		var result = cache.TryGetValue(key, out object value);
		return new JsonResult(new { value = result ? value : "Empty" });
	}

	public async Task<IActionResult> OnPostFlushWikiCache()
	{
		await wikiPages.FlushCache();
		return BasePageRedirect("CacheControl");
	}

	public IActionResult OnPostClearAwardsCache()
	{
		awards.FlushCache();
		return BasePageRedirect("CacheControl");
	}
}
