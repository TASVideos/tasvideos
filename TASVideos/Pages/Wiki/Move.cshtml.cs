using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.MoveWikiPages)]
public class MoveModel(IWikiPages wikiPages, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public string OriginalPageName { get; set; } = "";

	[BindProperty]
	[ValidWikiPageName]
	public string DestinationPageName { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (!string.IsNullOrWhiteSpace(Path))
		{
			Path = Path.Trim('/');
			if (await wikiPages.Exists(Path))
			{
				OriginalPageName = Path;
				DestinationPageName = Path;
				return Page();
			}
		}

		return NotFound();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		OriginalPageName = OriginalPageName.Trim('/');
		DestinationPageName = DestinationPageName.Trim('/');

		if (!await wikiPages.CanMove(OriginalPageName, DestinationPageName))
		{
			ModelState.AddModelError("", "Either the original page does not exist, or the destination page already exists or has an invalid format.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await wikiPages.Move(OriginalPageName, DestinationPageName, User.GetUserId());

		if (!result)
		{
			ModelState.AddModelError("", "Unable to move page, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await publisher.SendWiki(
			$"Page {OriginalPageName} moved to [{DestinationPageName}]({{0}}) by {User.Name()}",
			"",
			DestinationPageName);

		return BaseRedirect("/" + DestinationPageName);
	}
}
