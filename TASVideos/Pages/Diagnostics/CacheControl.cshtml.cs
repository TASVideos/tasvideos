using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Diagnostics
{
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
			return RedirectToPage("CacheControl");
		}

		public IActionResult OnPostClearAwardsCache()
		{
			_awards.FlushCache();
			return RedirectToPage("CacheControl");
		}
	}
}
