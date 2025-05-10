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

		if (await wikiPages.Exists(DestinationPageName, includeDeleted: true))
		{
			ModelState.AddModelError("DestinationPageName", "The destination page already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await wikiPages.Move(OriginalPageName, DestinationPageName);

		// Add a dummy commit to track the move
		var page = (await wikiPages.Page(DestinationPageName))!;
		await wikiPages.Add(new WikiCreateRequest
		{
			PageName = DestinationPageName,
			Markup = page.Markup,
			RevisionMessage = $"Page Moved from {OriginalPageName} to {DestinationPageName}",
			AuthorId = User.GetUserId(),
			MinorEdit = false
		});

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
