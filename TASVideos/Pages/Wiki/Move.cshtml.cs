﻿using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Models;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.MoveWikiPages)]
public class MoveModel(
	IWikiPages wikiPages,
	ExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public string OriginalPageName { get; set; } = "";

	[BindProperty]
	[ValidWikiPageName]
	[Display(Name = "Destination Page Name")]
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
			ModelState.AddModelError("Move.DestinationPageName", "The destination page already exists.");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await wikiPages.Move(OriginalPageName, DestinationPageName);

		if (!result)
		{
			ModelState.AddModelError("", "Unable to move page, the page may have been modified during the saving of this operation.");
			return Page();
		}

		await publisher.SendGeneralWiki(
			$"Page {OriginalPageName} moved to {DestinationPageName} by {User.Name()}",
			$"Page {OriginalPageName} moved to [{DestinationPageName}]({{0}}) by {User.Name()}",
			"",
			WikiHelper.EscapeUserName(DestinationPageName));

		return BaseRedirect("/" + DestinationPageName);
	}
}
