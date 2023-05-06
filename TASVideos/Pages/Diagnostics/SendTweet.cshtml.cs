using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SendTweetModel : BasePageModel
{
	[Required]
	[StringLength(280, MinimumLength = 1)]
	[BindProperty]
	public string Text { get; set; } = "";

	public void OnGet()
	{
		var twitter = HttpContext.RequestServices.GetService<TwitterDistributorV2>();
		if (twitter is null || !twitter.IsEnabled())
		{
			ModelState.AddModelError("", "Twitter is not enabled");
		}
	}

	public async Task<IActionResult> OnPost()
	{
		var twitter = HttpContext.RequestServices.GetService<TwitterDistributorV2>();
		if (twitter is null || !twitter.IsEnabled())
		{
			ModelState.AddModelError("", "Twitter is not enabled");
			return Page();
		}

		await twitter.Post(new Post
		{
			Body = Text,
			Title = "Test Message"
		});

		return RedirectToPage("SendTweet");
	}
}
