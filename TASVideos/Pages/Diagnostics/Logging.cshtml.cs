using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class LoggingModel : BasePageModel
{
	private readonly ILogger<ILogger> _logger;

	public LoggingModel(ILogger<ILogger> logger)
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}

	public IActionResult OnGetTestLogLevels()
	{
		_logger.LogTrace("This is a trace log.");
		_logger.LogDebug("This is a debug log.");
		_logger.LogInformation("This in an information log.");
		_logger.LogWarning("This is a warning log.");
		_logger.LogError("This is an error log.");
		_logger.LogCritical("This is a critical log.");
		return BasePageRedirect("Logging");
	}
}
