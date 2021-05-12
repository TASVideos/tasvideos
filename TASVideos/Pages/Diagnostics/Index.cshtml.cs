using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Data.Entity;

namespace TASVideos.RazorPages.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class IndexModel : PageModel
	{
		public void OnGet()
		{
		}
	}
}
