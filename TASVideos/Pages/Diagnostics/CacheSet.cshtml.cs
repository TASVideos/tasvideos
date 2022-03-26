using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheSetModel : PageModel
{
	private readonly ICacheService _cache;

	public CacheSetModel(ICacheService cache)
	{
		_cache = cache;
	}

	[BindProperty]
	public CacheRequest CacheEntry { get; set; } = new("", "");

	public void OnGet()
	{
	}

	public IActionResult OnPost()
	{
		_cache.Set(CacheEntry.Key, CacheEntry.Value);
		return RedirectToPage("CacheControl");
	}

	public record CacheRequest(string Key, string Value);
}