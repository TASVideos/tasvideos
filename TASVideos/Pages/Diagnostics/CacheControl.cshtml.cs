using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class CacheControlModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly AwardTasks _awardTasks;

		public CacheControlModel(
			IWikiPages wikiPages,
			AwardTasks awardTasks,
			UserManager userManager)
			: base(userManager)
		{
			_wikiPages = wikiPages;
			_awardTasks = awardTasks;
		}

		public async Task<IActionResult> OnPostFlushWikiCache()
		{
			await _wikiPages.FlushCache();
			return RedirectToPage("CacheControl");
		}

		public IActionResult OnPostClearAwardsCache()
		{
			// TODO: implement flush instead of clear
			_awardTasks.ClearAwardsCache();
			return RedirectToPage("CacheControl");
		}
	}
}
