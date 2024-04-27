using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TASVideos.Pages;

[IgnoreAntiforgeryToken]
public class ErrorModel(IHostEnvironment env) : PageModel
{
	public string StatusCodeString { get; set; } = "";
	public string ExceptionMessage { get; set; } = "";
	public List<KeyValuePair<string, StringValues>> RecoveredFormData { get; set; } = [];
	public void OnGet()
	{
		HandleStatusCode();
		HandleException();
	}

	public void OnPost()
	{
		HandleStatusCode();
		HandleException();
		HandleFormData();
	}

	private void HandleStatusCode()
	{
		StatusCodeString = $"Status Code {Response.StatusCode} - {ReasonPhrases.GetReasonPhrase(Response.StatusCode)}";
	}

	private void HandleException()
	{
		if (env.IsDevelopment() || User.Has(PermissionTo.SeeDiagnostics))
		{
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			ExceptionMessage = exceptionHandlerPathFeature?.Error.ToString() ?? "";
		}
	}

	private void HandleFormData()
	{
		if (Request.HasFormContentType)
		{
			RecoveredFormData.AddRange(Request.Form
				.Where(kvp => kvp.Key != "__RequestVerificationToken"));
		}
	}
}
