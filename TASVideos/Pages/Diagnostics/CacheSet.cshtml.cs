namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheSetModel(ICacheService cache) : BasePageModel
{
	[BindProperty]
	public CacheRequest CacheEntry { get; set; } = new("", "");

	public void OnGet()
	{
	}

	public IActionResult OnPost()
	{
		cache.Set(CacheEntry.Key, CacheEntry.Value);
		return RedirectToPage("CacheControl");
	}

	public record CacheRequest(string Key, string Value);
}
