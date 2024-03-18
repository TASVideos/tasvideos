using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Email;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SendEmail(IEmailService emailService) : BasePageModel
{
	[StringLength(100, MinimumLength = 1)]
	[BindProperty]
	public string To { get; set; } = "";

	[StringLength(100, MinimumLength = 1)]
	[BindProperty]
	public string Subject { get; set; } = "";

	[StringLength(280, MinimumLength = 1)]
	[BindProperty]
	public string Text { get; set; } = "";

	public async Task<IActionResult> OnPost()
	{
		await emailService.SendEmail(To, Subject, Text);
		return RedirectToPage("SendEmail");
	}
}
