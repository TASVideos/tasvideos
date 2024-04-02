namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class CacheSetModel(ICacheService cache) : BasePageModel
{
	[BindProperty]
	public string Key { get; set; } = "";

	[BindProperty]
	public string Value { get; set; } = "";

	public IActionResult OnPost()
	{
		cache.Set(Key, Value);
		return RedirectToPage("CacheControl");
	}
}
