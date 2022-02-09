using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class IndexModel : BasePageModel
{
	public void OnGet()
	{
	}

	public void OnPostMake500()
	{
		throw new Exception("Testing 500 exceptions from Diagnostics page.");
	}
}
