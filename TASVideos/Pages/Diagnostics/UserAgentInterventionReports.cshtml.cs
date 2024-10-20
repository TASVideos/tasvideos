namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class UserAgentInterventionReportsModel(ILogger<ILogger> logger) : BasePageModel
{
	public void OnPost()
	{
		logger.LogInformation("got POST payload maybe?");
	}
}
