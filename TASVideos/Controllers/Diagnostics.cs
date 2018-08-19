using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class DiagnosticsController : BaseController
	{
		private readonly WikiTasks _wikiTasks;
		private readonly AwardTasks _awardTasks;

		public DiagnosticsController(
			WikiTasks wikiTasks,
			AwardTasks awardTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
			_awardTasks = awardTasks;
		}

		public IActionResult Index()
		{
			var data = new
			{
				Version,
				Environment.MachineName,
				Environment.Is64BitOperatingSystem,
				OSVersion = Environment.OSVersion.ToString(),
				Environment.ProcessorCount,
				Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
				ProcessInfo = GetProcessInfo()
			};

			return Ok(data);
		}

		public IActionResult CacheControl()
		{
			return View();
		}

		[HttpPost]
		public IActionResult ClearWikiCache()
		{
			_wikiTasks.ClearWikiCache();
			return Json(new { Success = true });
		}

		public IActionResult ClearAwardsCache()
		{
			_awardTasks.ClearAwardsCache();
			return Json(new { Success = true });
		}

		private object GetProcessInfo()
		{
			try
			{
				var process = Process.GetCurrentProcess();

				return new
				{

					TotalMemoryUsage = $"{(process.PrivateMemorySize64 / 1024 / 1024):n0} MB"
				};
			}
			catch (PlatformNotSupportedException)
			{
				return "Platform Not Supported";
			}
		}
	}
}
