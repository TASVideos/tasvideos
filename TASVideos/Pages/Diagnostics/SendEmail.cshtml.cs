using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Email;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SendEmail : BasePageModel
{
	private readonly IEmailService _emailService;

	public SendEmail(IEmailService emailService)
	{
		_emailService = emailService;
	}

	[Required]
	[StringLength(100, MinimumLength = 1)]
	[BindProperty]
	public string To { get; set; } = "";

	[Required]
	[StringLength(100, MinimumLength = 1)]
	[BindProperty]
	public string Subject { get; set; } = "";

	[Required]
	[StringLength(280, MinimumLength = 1)]
	[BindProperty]
	public string Text { get; set; } = "";

	public async Task<IActionResult> OnPost()
	{
		await _emailService.SendEmail(To, Subject, Text);
		return RedirectToPage("SendEmail");
	}
}
