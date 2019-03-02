using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class IndexModel : PageModel
	{
		public void OnGet()
		{
		}
	}
}
