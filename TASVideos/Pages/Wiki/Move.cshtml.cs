using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.MoveWikiPages)]
public class MoveModel(
	IWikiPages wikiPages,
	ExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public WikiMoveModel Move { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (!string.IsNullOrWhiteSpace(Path))
		{
			Path = Path.Trim('/');
			if (await wikiPages.Exists(Path))
			{
				Move = new WikiMoveModel
				{
					OriginalPageName = Path,
					DestinationPageName = Path
				};
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

		Move.OriginalPageName = Move.OriginalPageName.Trim('/');
		Move.DestinationPageName = Move.DestinationPageName.Trim('/');

		if (await wikiPages.Exists(Move.DestinationPageName, includeDeleted: true))
		{
			ModelState.AddModelError("Move.DestinationPageName", "The destination page already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await wikiPages.Move(Move.OriginalPageName, Move.DestinationPageName);

		if (!result)
		{
			ModelState.AddModelError("", "Unable to move page, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await publisher.SendGeneralWiki(
			$"Page {Move.OriginalPageName} moved to {Move.DestinationPageName} by {User.Name()}",
			$"Page {Move.OriginalPageName} moved to [{Move.DestinationPageName}]({{0}}) by {User.Name()}",
			"",
			WikiHelper.EscapeUserName(Move.DestinationPageName));

		return BaseRedirect("/" + Move.DestinationPageName);
	}
}
