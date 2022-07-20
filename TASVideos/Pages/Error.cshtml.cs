using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TASVideos.Pages;

[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
	private readonly IHostEnvironment _env;
	public ErrorModel(IHostEnvironment env)
	{
		_env = env;
	}

	public string StatusCodeString { get; set; } = "";
	public string ExceptionMessage { get; set; } = "";
	public List<KeyValuePair<string, StringValues>> RecoveredFormData { get; set; } = new();
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
		if (_env.IsDevelopment())
		{
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			ExceptionMessage = exceptionHandlerPathFeature?.Error.ToString() ?? "";
		}
	}

	private void HandleFormData()
	{
		RecoveredFormData.AddRange(Request.Form
			.Where(kvp => kvp.Key != "__RequestVerificationToken"));
	}
}
