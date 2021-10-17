using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class IndexModel : BasePageModel
	{
		public void OnGet()
		{
		}
	}
}
