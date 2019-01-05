using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class DiagnosticsController : BaseController
	{
		private readonly IWikiPages _wikiPages;

		private readonly AwardTasks _awardTasks;

		public DiagnosticsController(
			AwardTasks awardTasks,
			UserTasks userTasks,
			IWikiPages wikiPages)
			: base(userTasks)
		{
			_awardTasks = awardTasks;
			_wikiPages = wikiPages;
		}

		
		public IActionResult CacheControl()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> FlushWikiCache()
		{
			await _wikiPages.FlushCache();
			return Ok();
		}

		[HttpPost]
		public IActionResult ClearAwardsCache()
		{
			_awardTasks.ClearAwardsCache();
			return Ok();
		}
	}
}
