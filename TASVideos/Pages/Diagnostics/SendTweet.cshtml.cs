using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SendTweetModel : PageModel
{
	private readonly TwitterDistributorV2 _twitterDistributor;

	public SendTweetModel(TwitterDistributorV2 twitterDistributor)
	{
		_twitterDistributor = twitterDistributor;
	}

	[Required]
	[StringLength(280, MinimumLength = 1)]
	[BindProperty]
	public string Text { get; set; } = "";

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPost()
	{
		await _twitterDistributor.Post(new Post
		{
			Body = Text,
			Title = "Test Message"
		});

		return RedirectToPage("SendTweet");
	}
}