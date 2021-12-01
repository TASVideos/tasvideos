using System;
using Microsoft.Extensions.Logging;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class IndexModel : BasePageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public void OnGet()
		{
		}

		public void OnPostMake500()
		{
			_logger.LogTrace("This is a trace log.");
			_logger.LogDebug("This is a debug log.");
			_logger.LogInformation("This in an information log.");
			_logger.LogWarning("This is a warning log.");
			_logger.LogError("This is an error log.");
			_logger.LogCritical("This is a critical log.");

			throw new Exception("Testing 500 exceptions from Diagnostics page.");
		}
	}
}
