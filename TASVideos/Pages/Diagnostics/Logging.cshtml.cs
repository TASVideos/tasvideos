namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class LoggingModel(ILogger<ILogger> logger) : BasePageModel
{
	public IActionResult OnGetTestLogLevels()
	{
		logger.LogTrace("This is a trace log.");
		logger.LogDebug("This is a debug log.");
		logger.LogInformation("This in an information log.");
		logger.LogWarning("This is a warning log.");
		logger.LogError("This is an error log.");
		logger.LogCritical("This is a critical log.");
		return BasePageRedirect("Logging");
	}
}
